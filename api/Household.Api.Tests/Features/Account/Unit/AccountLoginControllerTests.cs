using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Controllers;
using Household.Api.Tests.Features.Account.Data;
using Household.Api.Tests.Features.Account.Support;
using Microsoft.AspNetCore.Mvc;

namespace Household.Api.Tests.Features.Account.Unit;

[Trait("Category", "Unit")]
public sealed class AccountLoginControllerTests
{
    [Theory]
    [ClassData(typeof(AccountControllerCases.LoginResults))]
    public async Task Login_ForwardsRequestAndToken_AndReturnsServiceStatus(LoginStatus serviceResult, int expectedStatus)
    {
        // Arrange: choose what the service returns, independently of any account rules.
        var lockoutEnd = serviceResult == LoginStatus.LockedOut
            ? new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.Zero) : (DateTimeOffset?)null;
        var service = new AccountServiceStub { LoginResult = new(serviceResult, lockoutEnd) };
        var controller = new AccountController(service);
        var request = new LoginRequest(LoginMode.SessionCookie, "user@example.com", "Password123!");
        using var source = new CancellationTokenSource();

        // Act: call the controller directly. No HTTP server is involved.
        var result = await controller.Login(request, source.Token);

        // Assert: preserve the business outcome and optional lockout deadline in the response.
        var call = Assert.Single(service.Calls);
        Assert.Same(request, call.Request);
        Assert.Equal(source.Token, call.Token);
        var status = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(expectedStatus, status.StatusCode);
        var response = Assert.IsType<LoginResponse>(status.Value);
        Assert.Equal(serviceResult, response.Status);
        Assert.Equal(lockoutEnd, response.LockoutEnd);
    }

    [Fact]
    public async Task Login_WhenTokenSignInSucceeds_LeavesIdentityResponseUntouched()
    {
        var controller = new AccountController(new AccountServiceStub());

        var result = await controller.Login(
            new LoginRequest(LoginMode.Token, "user@example.com", "Password123!"), CancellationToken.None);

        // Token issuance is verified through real Identity in the integration tests.
        Assert.IsType<EmptyResult>(result);
    }

    [Fact]
    public async Task Login_WhenServiceReturnsUnknownStatus_ReturnsServerError()
    {
        var controller = new AccountController(new AccountServiceStub
        {
            LoginResult = new((LoginStatus)999)
        });

        var result = await controller.Login(
            new LoginRequest(LoginMode.SessionCookie, "user@example.com", "Password123!"), CancellationToken.None);

        Assert.Equal(500, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }

    [Fact]
    public async Task Login_WithCancelledToken_PropagatesCancellation()
    {
        var service = new AccountServiceStub();
        var controller = new AccountController(service);
        var request = new LoginRequest(LoginMode.SessionCookie, "user@example.com", "Password123!");
        using var source = new CancellationTokenSource();
        source.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => controller.Login(request, source.Token));

        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.Equal(source.Token, Assert.Single(service.Calls).Token);
    }
}
