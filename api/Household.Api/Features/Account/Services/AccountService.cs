using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Options;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly SignInManager<ApplicationIdentityUser> _signInManager;
    private readonly ILogger<AccountService> _logger;
    private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;
    private readonly TimeProvider _timeProvider;

    public AccountService(UserManager<ApplicationIdentityUser> userManager, 
        SignInManager<ApplicationIdentityUser> signInManager, 
        ILogger<AccountService> logger,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _bearerTokenOptions = bearerTokenOptions;
        _timeProvider = timeProvider;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var authenticationScheme = request.Mode switch
        {
            LoginMode.Token => IdentityConstants.BearerScheme,
            LoginMode.SessionCookie or LoginMode.PersistentCookie => IdentityConstants.ApplicationScheme,
            _ => throw new ArgumentException("A valid login mode is required.", nameof(request))
        };

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return new(LoginStatus.InvalidCredentials);

        var restriction = await CheckLoginRestrictionsAsync(user, request.Password);
        if (restriction is not null) return restriction;

        cancellationToken.ThrowIfCancellationRequested();
        _signInManager.AuthenticationScheme = authenticationScheme;

        var result = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: request.Mode == LoginMode.PersistentCookie,
            lockoutOnFailure: true
        );

        if (result.Succeeded) return new(LoginStatus.Succeeded);
        if (result.IsLockedOut) return new(LoginStatus.LockedOut, await _userManager.GetLockoutEndDateAsync(user));
        if (result.RequiresTwoFactor) return new(LoginStatus.RequiresTwoFactor);
        if (result.IsNotAllowed) return new(LoginStatus.NotAllowed);

        return new(LoginStatus.InvalidCredentials);
    }

    private async Task<LoginResult?> CheckLoginRestrictionsAsync(ApplicationIdentityUser user, string password)
    {
        if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
            return new(LoginStatus.LockedOut, await _userManager.GetLockoutEndDateAsync(user));

        if (await _signInManager.CanSignInAsync(user))
            return null;

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            if (_userManager.SupportsUserLockout && await _userManager.GetLockoutEnabledAsync(user))
            {
                var failureResult = await _userManager.AccessFailedAsync(user);

                if (!failureResult.Succeeded)
                    return new(LoginStatus.InvalidCredentials);

                if (await _userManager.IsLockedOutAsync(user))
                    return new(LoginStatus.LockedOut, await _userManager.GetLockoutEndDateAsync(user));
            }

            return new(LoginStatus.InvalidCredentials);
        }

        if (_signInManager.Options.SignIn.RequireConfirmedEmail && !await _userManager.IsEmailConfirmedAsync(user))
            return new(LoginStatus.EmailNotConfirmed);

        return new(LoginStatus.NotAllowed);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _signInManager.AuthenticationScheme = IdentityConstants.ApplicationScheme;
        await _signInManager.SignOutAsync();
    }

    public async Task<CreateUserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null) return new(CreateUserStatus.AlreadyExists, EmailConfirmed: existingUser.EmailConfirmed);

        var user = new ApplicationIdentityUser { UserName = request.Email, Email = request.Email };
        var identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            var isDuplicate = identityResult.Errors.Any(
                    error => error.Code is nameof(IdentityErrorDescriber.DuplicateEmail) or nameof(IdentityErrorDescriber.DuplicateUserName)
                );

            if (!isDuplicate) return new(CreateUserStatus.ValidationFailed, IdentityResult: identityResult);

            var duplicateUser = await _userManager.FindByEmailAsync(request.Email);
            return new(CreateUserStatus.AlreadyExists, IdentityResult: identityResult, EmailConfirmed: duplicateUser?.EmailConfirmed);
        }

        try
        {
            var confirmationResult = await SendConfirmationAsync(new ResendConfirmationRequest(request.Email), cancellationToken);
            if (confirmationResult != SendConfirmationStatus.Sent) return new(CreateUserStatus.CreatedConfirmationFailed, ConfirmationStatus: confirmationResult);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Confirmation failed after creating user {UserId}.", user.Id);
            return new(CreateUserStatus.CreatedConfirmationFailed, ConfirmationStatus: SendConfirmationStatus.Failed);
        }

        return new(CreateUserStatus.Created);
    }

    public async Task<SendConfirmationStatus> SendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return SendConfirmationStatus.UserNotFound;
        if (user.EmailConfirmed) return SendConfirmationStatus.AlreadyConfirmed;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        if (token.IsNullOrEmpty()) return SendConfirmationStatus.Failed;

        //TODO: build confirmation URL
        //TODO: send using configured email provider

        return SendConfirmationStatus.Sent;
    }

    public async Task<RefreshStatus> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return RefreshStatus.InvalidRefreshToken;

        var options = _bearerTokenOptions.Get(IdentityConstants.BearerScheme);
        var ticket = options.RefreshTokenProtector.Unprotect(request.RefreshToken);

        if (ticket is null || !ticket.Properties.ExpiresUtc.HasValue || ticket.Properties.ExpiresUtc.Value <= _timeProvider.GetUtcNow())
            return RefreshStatus.InvalidRefreshToken;

        var user = await _signInManager.ValidateSecurityStampAsync(ticket.Principal);

        if (user is null)
            return RefreshStatus.InvalidRefreshToken;

        if (!await _signInManager.CanSignInAsync(user))
            return RefreshStatus.InvalidRefreshToken;

        if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
            return RefreshStatus.InvalidRefreshToken;

        cancellationToken.ThrowIfCancellationRequested();

        _signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RefreshStatus.Succeeded;
    }
}
