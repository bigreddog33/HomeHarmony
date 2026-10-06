using Household.Api.Features.Account.Contracts;
using Household.Api.Tests.Features.Account.Data;
using Household.Api.Tests.Features.Account.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Household.Api.Tests.Features.Account.Unit;

[Trait("Category", "Unit")]
public sealed class AccountServiceTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private readonly IdentityUserManagerStub _userManager;
    private readonly AccountService _service;
    private readonly CreateUserRequest _request = new("user@example.com", "Password123!");

    public AccountServiceTests()
    {
        // Use Identity's normal dependency wiring, replacing only UserManager.
        // No database, HTTP server, or email provider is registered.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication();
        services.AddIdentityCore<ApplicationIdentityUser>().AddSignInManager();
        services.AddScoped<UserManager<ApplicationIdentityUser>, IdentityUserManagerStub>();
        services.AddScoped<AccountService>();
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        _scope = _provider.CreateScope();
        _userManager = (IdentityUserManagerStub)_scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationIdentityUser>>();
        _service = _scope.ServiceProvider.GetRequiredService<AccountService>();
    }

    [Fact]
    public async Task CreateUser_WhenEmailAlreadyExists_DoesNotCreateOrConfirmUser()
    {
        _userManager.FindResults.Enqueue(new ApplicationIdentityUser { Email = _request.Email });

        var result = await _service.CreateUserAsync(_request, CancellationToken.None);

        Assert.Equal(CreateUserStatus.AlreadyExists, result.Status);
        Assert.Equal(_request.Email, Assert.Single(_userManager.EmailLookups));
        Assert.Empty(_userManager.CreateCalls);
        Assert.Empty(_userManager.ConfirmationCalls);
    }

    [Theory]
    [ClassData(typeof(AccountServiceCases.DuplicateErrors))]
    public async Task CreateUser_WhenIdentityDetectsDuplicateAfterLookup_ReturnsAlreadyExists(string errorCode)
    {
        _userManager.FindResults.Enqueue(null);
        _userManager.FindResults.Enqueue(new ApplicationIdentityUser { Email = _request.Email });
        _userManager.CreateResult = IdentityResult.Failed(new IdentityError
        {
            Code = errorCode,
            Description = "An account with this email or username already exists."
        });

        var result = await _service.CreateUserAsync(_request, CancellationToken.None);

        Assert.Equal(CreateUserStatus.AlreadyExists, result.Status);
        Assert.Same(_userManager.CreateResult, result.IdentityResult);
        Assert.Single(_userManager.CreateCalls);
        Assert.Equal(new[] { _request.Email, _request.Email }, _userManager.EmailLookups);
        Assert.Empty(_userManager.ConfirmationCalls);
    }

    [Fact]
    public async Task CreateUser_WhenIdentityRejectsUser_PreservesAllErrorsAndSkipsConfirmation()
    {
        _userManager.FindResults.Enqueue(null);
        var errors = new IdentityError[]
        {
            new() { Code = "PasswordTooShort", Description = "Password is too short." },
            new() { Code = "PasswordRequiresDigit", Description = "Password requires a digit." }
        };
        _userManager.CreateResult = IdentityResult.Failed(errors);

        var result = await _service.CreateUserAsync(_request, CancellationToken.None);

        Assert.Equal(CreateUserStatus.ValidationFailed, result.Status);
        Assert.NotNull(result.IdentityResult);
        Assert.False(result.IdentityResult.Succeeded);
        Assert.Equal(errors, result.IdentityResult.Errors);
        Assert.Single(_userManager.CreateCalls);
        Assert.Single(_userManager.EmailLookups);
        Assert.Empty(_userManager.ConfirmationCalls);
    }

    [Fact]
    public async Task CreateUser_WhenCreationAndTokenGenerationSucceed_ReturnsCreated()
    {
        var createdUser = new ApplicationIdentityUser { Email = _request.Email };
        _userManager.FindResults.Enqueue(null);
        _userManager.FindResults.Enqueue(createdUser);

        var result = await _service.CreateUserAsync(_request, CancellationToken.None);

        Assert.Equal(CreateUserStatus.Created, result.Status);
        Assert.Null(result.IdentityResult);
        Assert.Null(result.ConfirmationStatus);
        var createCall = Assert.Single(_userManager.CreateCalls);
        Assert.Equal(_request.Email, createCall.User.Email);
        Assert.Equal(_request.Email, createCall.User.UserName);
        Assert.Equal(_request.Password, createCall.Password);
        Assert.Equal(new[] { _request.Email, _request.Email }, _userManager.EmailLookups);
        Assert.Same(createdUser, Assert.Single(_userManager.ConfirmationCalls));
    }

    [Theory]
    [ClassData(typeof(AccountServiceCases.UnsuccessfulConfirmations))]
    public async Task CreateUser_WhenConfirmationDoesNotSucceed_PreservesCreationAndFailureReason(
        SendConfirmationStatus expectedConfirmationStatus)
    {
        _userManager.FindResults.Enqueue(null);
        _userManager.FindResults.Enqueue(expectedConfirmationStatus == SendConfirmationStatus.UserNotFound
            ? null
            : new ApplicationIdentityUser
            {
                Email = _request.Email,
                EmailConfirmed = expectedConfirmationStatus == SendConfirmationStatus.AlreadyConfirmed
            });
        _userManager.ConfirmationToken = "";

        var result = await _service.CreateUserAsync(_request, CancellationToken.None);

        Assert.Equal(CreateUserStatus.CreatedConfirmationFailed, result.Status);
        Assert.Equal(expectedConfirmationStatus, result.ConfirmationStatus);
        Assert.Single(_userManager.CreateCalls);
        Assert.Equal(new[] { _request.Email, _request.Email }, _userManager.EmailLookups);
        if (expectedConfirmationStatus == SendConfirmationStatus.Failed)
            Assert.Single(_userManager.ConfirmationCalls);
        else
            Assert.Empty(_userManager.ConfirmationCalls);
    }

    [Fact]
    public async Task CreateUser_WithCancelledToken_DoesNotCallIdentity()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _service.CreateUserAsync(_request, source.Token));

        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.Empty(_userManager.EmailLookups);
        Assert.Empty(_userManager.CreateCalls);
        Assert.Empty(_userManager.ConfirmationCalls);
    }

    [Fact]
    public async Task SendConfirmation_WhenUserIsMissing_ReturnsUserNotFoundWithoutGeneratingToken()
    {
        _userManager.FindResults.Enqueue(null);

        var result = await _service.SendConfirmationAsync(
            new ResendConfirmationRequest(_request.Email), CancellationToken.None);

        Assert.Equal(SendConfirmationStatus.UserNotFound, result);
        Assert.Equal(_request.Email, Assert.Single(_userManager.EmailLookups));
        Assert.Empty(_userManager.ConfirmationCalls);
    }

    [Fact]
    public async Task SendConfirmation_WhenEmailIsConfirmed_DoesNotGenerateAnotherToken()
    {
        _userManager.FindResults.Enqueue(new ApplicationIdentityUser
        {
            Email = _request.Email,
            EmailConfirmed = true
        });

        var result = await _service.SendConfirmationAsync(
            new ResendConfirmationRequest(_request.Email), CancellationToken.None);

        Assert.Equal(SendConfirmationStatus.AlreadyConfirmed, result);
        Assert.Equal(_request.Email, Assert.Single(_userManager.EmailLookups));
        Assert.Empty(_userManager.ConfirmationCalls);
    }

    [Theory]
    [ClassData(typeof(AccountServiceCases.ConfirmationTokens))]
    public async Task SendConfirmation_ReturnsOutcomeForGeneratedToken(string token, SendConfirmationStatus expectedStatus)
    {
        var user = new ApplicationIdentityUser { Email = _request.Email };
        _userManager.FindResults.Enqueue(user);
        _userManager.ConfirmationToken = token;

        var result = await _service.SendConfirmationAsync(
            new ResendConfirmationRequest(_request.Email), CancellationToken.None);

        // Sent currently means a token was generated; email delivery is still a TODO.
        Assert.Equal(expectedStatus, result);
        Assert.Equal(_request.Email, Assert.Single(_userManager.EmailLookups));
        Assert.Same(user, Assert.Single(_userManager.ConfirmationCalls));
    }

    [Fact]
    public async Task SendConfirmation_WithCancelledToken_DoesNotCallIdentity()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _service.SendConfirmationAsync(new ResendConfirmationRequest(_request.Email), source.Token));

        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.Empty(_userManager.EmailLookups);
        Assert.Empty(_userManager.ConfirmationCalls);
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
    }
}
