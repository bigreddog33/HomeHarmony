using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly SignInManager<ApplicationIdentityUser> _signInManager;

    public AccountService(UserManager<ApplicationIdentityUser> userManager, SignInManager<ApplicationIdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<bool> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return false;

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true
        );

        return result.Succeeded;
    }

    public async Task<CreateUserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null) return new(CreateUserStatus.AlreadyExists);

        var user = new ApplicationIdentityUser { UserName = request.Email, Email = request.Email };
        var identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            var isDuplicate = identityResult.Errors.Any(
                    error => error.Code is nameof(IdentityErrorDescriber.DuplicateEmail) or nameof(IdentityErrorDescriber.DuplicateUserName)
                );

            return new(isDuplicate ? CreateUserStatus.AlreadyExists : CreateUserStatus.ValidationFailed, IdentityResult: identityResult);
        }

        var confirmationResult = await SendConfirmationAsync(new ResendConfirmationRequest(request.Email), cancellationToken);
        if (confirmationResult != SendConfirmationStatus.Sent) return new(CreateUserStatus.CreatedConfirmationFailed, ConfirmationStatus: confirmationResult);

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
}