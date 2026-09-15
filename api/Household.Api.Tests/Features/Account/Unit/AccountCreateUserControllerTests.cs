using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Controllers;
using Household.Api.Tests.Features.Account.Data;
using Household.Api.Tests.Features.Account.Support;
using Microsoft.AspNetCore.Mvc;

namespace Household.Api.Tests.Features.Account.Unit;

[Trait("Category", "Unit")]
public sealed class AccountCreateUserControllerTests
{
    [Theory]
    [ClassData(typeof(AccountControllerCases.ServiceResults))]
    public async Task CreateUser_ForwardsRequestAndToken_AndReturnsServiceStatus(bool serviceResult, int expectedStatus)
    {
        // Arrange: choose what the service returns, independently of any account rules.
        var service = new AccountServiceStub { Result = serviceResult };
        var controller = new AccountController(service);
        var request = new CreateUserRequest("user@example.com", "Password123!");
        using var source = new CancellationTokenSource();

        // Act: call the controller directly. No HTTP server is involved.
        var result = await controller.CreateUser(request, source.Token);

        // Assert: the service received this request/token once, and the response has no body.
        var call = Assert.Single(service.Calls);
        Assert.Same(request, call.Request);
        Assert.Equal(source.Token, call.Token);
        var status = Assert.IsAssignableFrom<StatusCodeResult>(result);
        Assert.Equal(expectedStatus, status.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithCancelledToken_PropagatesCancellation()
    {
        var service = new AccountServiceStub();
        var controller = new AccountController(service);
        var request = new CreateUserRequest("user@example.com", "Password123!");
        using var source = new CancellationTokenSource();
        source.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => controller.CreateUser(request, source.Token));

        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.Equal(source.Token, Assert.Single(service.Calls).Token);
    }
}
