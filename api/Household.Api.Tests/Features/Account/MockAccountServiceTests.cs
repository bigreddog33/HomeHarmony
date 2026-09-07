using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Services;

namespace Household.Api.Tests.Features.Account;

public sealed class MockAccountServiceTests
{
    private readonly MockAccountService _accountService = new();

    [Theory]
    [InlineData("demo@homeharmony.local")]
    [InlineData("DEMO@HOMEHARMONY.LOCAL")]
    public async Task LoginAsync_WithDemoCredentials_ReturnsTrue(string email)
    {
        var request = new LoginRequest(email, "Password123!");

        var isValid = await _accountService.LoginAsync(request, CancellationToken.None);

        Assert.True(isValid);
    }

    [Theory]
    [MemberData(nameof(LoginTestCases.InvalidCredentials), MemberType = typeof(LoginTestCases))]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsFalse(string email, string password)
    {
        var request = new LoginRequest(email, password);

        var isValid = await _accountService.LoginAsync(request, CancellationToken.None);

        Assert.False(isValid);
    }

    [Fact]
    public async Task LoginAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        var request = new LoginRequest("demo@homeharmony.local", "Password123!");
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _accountService.LoginAsync(request, cancellationTokenSource.Token));
    }
}
