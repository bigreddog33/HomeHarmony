using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Controllers;
using Household.Api.Features.Account.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Household.Api.Tests.Features.Account;

public sealed class AccountControllerTests
{
    private readonly AccountController _controller = new(new MockAccountService());

    [Fact]
    public async Task Login_WithDemoCredentials_ReturnsOkWithoutResponseBody()
    {
        var request = new LoginRequest("demo@homeharmony.local", "Password123!");

        var result = await _controller.Login(request, CancellationToken.None);

        var okResult = Assert.IsType<OkResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [Theory]
    [MemberData(nameof(LoginTestCases.InvalidCredentials), MemberType = typeof(LoginTestCases))]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized(string email, string password)
    {
        var request = new LoginRequest(email, password);

        var result = await _controller.Login(request, CancellationToken.None);

        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task Login_WhenCancelled_PassesCancellationToAccountService()
    {
        var request = new LoginRequest("demo@homeharmony.local", "Password123!");
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _controller.Login(request, cancellationTokenSource.Token));
    }
}
