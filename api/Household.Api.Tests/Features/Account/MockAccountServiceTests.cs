using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Services;
namespace Household.Api.Tests.Features.Account;

public sealed class MockAccountServiceTests
{
    private readonly MockAccountService _accountService = new();

    [Fact]
    public async Task LoginAsync_WithDemoCredentials_ReturnsSuccessfulResponse()
    {
        var request = new LoginRequest("demo@homeharmony.local", "Password123!");
        var response = await _accountService.LoginAsync(request, CancellationToken.None);
        Assert.NotNull(response);
    }

    [Theory]
    [MemberData(nameof(LoginTestCases.InvalidCredentials), MemberType = typeof(LoginTestCases))]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsNull(string email, string password)
    {
        var request = new LoginRequest(email, password);
        var response = await _accountService.LoginAsync(request, CancellationToken.None);
        Assert.Null(response);
    }
}