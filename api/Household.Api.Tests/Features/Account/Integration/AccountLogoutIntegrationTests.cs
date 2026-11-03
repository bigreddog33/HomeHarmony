using System.Net;
using System.Net.Http.Json;
using Household.Api.Features.Account.Contracts;
using Household.Api.Tests.Features.Account.Support;
using static Household.Api.Tests.Features.Account.Support.AuthenticationAssertions;
using static Household.Api.Tests.Features.Account.Support.IdentityAccountApiFactory;

namespace Household.Api.Tests.Features.Account.Integration;

[Trait("Category", "Integration")]
public sealed class AccountLogoutIntegrationTests : IDisposable
{
    private readonly IdentityAccountApiFactory _factory = new();
    private readonly HttpClient _client;

    public AccountLogoutIntegrationTests()
    {
        _client = _factory.CreateApiClient();
    }

    [Theory]
    [InlineData(LoginMode.SessionCookie)]
    [InlineData(LoginMode.PersistentCookie)]
    public async Task Logout_DeletesCookieAndBrowserLosesAccess_AndCanBeRepeated(LoginMode mode)
    {
        var user = await _factory.CreateUserAsync();
        using var login = await LoginAsync(_client, mode);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        await AuthenticatedAsync(_client, user.Id);

        using var logout = await _client.PostAsJsonAsync("/api/account/logout", new LogoutRequest());

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Empty(await logout.Content.ReadAsByteArrayAsync());
        var cookie = Assert.Single(Cookies(logout), cookie => cookie.Name.Value == _factory.ApplicationCookieName);
        Assert.True(string.IsNullOrEmpty(cookie.Value.Value));
        Assert.NotNull(cookie.Expires);
        Assert.True(cookie.Expires.Value < DateTimeOffset.UtcNow);
        await UnauthorizedAsync(_client);

        using var repeated = await _client.PostAsJsonAsync("/api/account/logout", new LogoutRequest());
        Assert.Equal(HttpStatusCode.NoContent, repeated.StatusCode);
        await UnauthorizedAsync(_client);
    }

    [Fact]
    public async Task Logout_WithoutAuthentication_Succeeds()
    {
        using var response = await _client.PostAsJsonAsync("/api/account/logout", new LogoutRequest());

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Logout_WithExpiredCookie_StillClearsIt()
    {
        await _factory.CreateUserAsync();
        using var login = await LoginAsync(_client, LoginMode.PersistentCookie);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        _factory.Clock.Advance(TimeSpan.FromDays(8));
        await UnauthorizedAsync(_client);

        using var response = await _client.PostAsJsonAsync("/api/account/logout", new LogoutRequest());

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains(Cookies(response), cookie => cookie.Name.Value == _factory.ApplicationCookieName
            && string.IsNullOrEmpty(cookie.Value.Value));
    }

    [Fact]
    public async Task Logout_DoesNotRevokeExistingBearerTokens()
    {
        var user = await _factory.CreateUserAsync();
        var tokens = await LoginTokensAsync(_client);

        using var logout = await _client.PostAsJsonAsync("/api/account/logout", new LogoutRequest());

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        // Mobile logout removes local credentials. Server-side revocation is deferred.
        await AuthenticatedAsync(_client, user.Id, tokens.AccessToken);
        using var refresh = await _client.PostAsJsonAsync("/api/account/refresh", new RefreshRequest(tokens.RefreshToken));
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
