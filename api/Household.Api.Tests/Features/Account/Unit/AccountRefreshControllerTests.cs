using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Controllers;
using Household.Api.Tests.Features.Account.Support;
using Microsoft.AspNetCore.Mvc;

namespace Household.Api.Tests.Features.Account.Unit;

[Trait("Category", "Unit")]
public sealed class AccountRefreshControllerTests
{
    [Theory]
    [InlineData(RefreshStatus.Succeeded)]
    [InlineData(RefreshStatus.InvalidRefreshToken)]
    public async Task Refresh_ForwardsRequestAndToken_AndMapsOutcome(RefreshStatus outcome)
    {
        var service = new AccountServiceStub { RefreshResult = outcome };
        var controller = new AccountController(service);
        var request = new RefreshRequest("refresh-token");
        using var source = new CancellationTokenSource();

        var result = await controller.Refresh(request, source.Token);

        var call = Assert.Single(service.Calls);
        Assert.Same(request, call.Request);
        Assert.Equal(source.Token, call.Token);
        if (outcome == RefreshStatus.Succeeded)
        {
            Assert.IsType<EmptyResult>(result);
        }
        else
        {
            var response = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(outcome, Assert.IsType<RefreshResponse>(response.Value).Status);
        }
    }

    [Fact]
    public async Task Refresh_WhenServiceReturnsUnknownStatus_ReturnsServerError()
    {
        var controller = new AccountController(new AccountServiceStub { RefreshResult = (RefreshStatus)999 });

        var result = await controller.Refresh(new RefreshRequest("refresh-token"), CancellationToken.None);

        Assert.Equal(500, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }

    [Fact]
    public async Task Refresh_WithCancelledToken_PropagatesCancellation()
    {
        var service = new AccountServiceStub();
        var controller = new AccountController(service);
        using var source = new CancellationTokenSource();
        source.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => controller.Refresh(new RefreshRequest("refresh-token"), source.Token));

        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.Equal(source.Token, Assert.Single(service.Calls).Token);
    }
}
