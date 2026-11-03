using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Controllers;
using Household.Api.Tests.Features.Account.Support;
using Microsoft.AspNetCore.Mvc;

namespace Household.Api.Tests.Features.Account.Unit;

[Trait("Category", "Unit")]
public sealed class AccountLogoutControllerTests
{
    [Fact]
    public async Task Logout_ForwardsToken_AndReturnsNoContent()
    {
        var service = new AccountServiceStub();
        var controller = new AccountController(service);
        using var source = new CancellationTokenSource();

        var result = await controller.Logout(new LogoutRequest(), source.Token);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(source.Token, Assert.Single(service.Calls).Token);
    }

    [Fact]
    public async Task Logout_WithCancelledToken_PropagatesCancellation()
    {
        var service = new AccountServiceStub();
        var controller = new AccountController(service);
        using var source = new CancellationTokenSource();
        source.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => controller.Logout(new LogoutRequest(), source.Token));

        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.Equal(source.Token, Assert.Single(service.Calls).Token);
    }
}
