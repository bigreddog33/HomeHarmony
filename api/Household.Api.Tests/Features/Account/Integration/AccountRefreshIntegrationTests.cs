using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Household.Api.Tests.Features.Account.Data;
using Household.Api.Tests.Features.Account.Support;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static Household.Api.Tests.Features.Account.Support.AuthenticationAssertions;
using static Household.Api.Tests.Features.Account.Support.IdentityAccountApiFactory;
using static Household.Api.Tests.Features.Account.Data.AccountAuthenticationCases;

namespace Household.Api.Tests.Features.Account.Integration;

[Trait("Category", "Integration")]
public sealed class AccountRefreshIntegrationTests : IDisposable
{
    private readonly IdentityAccountApiFactory _factory = new();
    private readonly HttpClient _client;

    public AccountRefreshIntegrationTests()
    {
        _client = _factory.CreateApiClient();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Refresh_WithNoOrExpiredAccessToken_ReturnsUsableNewPair(bool sendExpiredAccessToken)
    {
        var user = await _factory.CreateUserAsync();
        var original = await LoginTokensAsync(_client);
        _factory.Clock.Advance(TimeSpan.FromMinutes(16));
        await UnauthorizedAsync(_client, original.AccessToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/account/refresh")
        {
            Content = JsonContent.Create(new RefreshRequest(original.RefreshToken))
        };
        if (sendExpiredAccessToken)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", original.AccessToken);

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var refreshed = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        Assert.NotNull(refreshed);
        Assert.Equal("Bearer", refreshed.TokenType);
        Assert.Equal(900, refreshed.ExpiresIn);
        Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));
        Assert.NotEqual(original.AccessToken, refreshed.AccessToken);
        Assert.NotEqual(original.RefreshToken, refreshed.RefreshToken);
        Assert.DoesNotContain(Cookies(response), cookie => cookie.Name.Value == _factory.ApplicationCookieName);
        await AuthenticatedAsync(_client, user.Id, refreshed.AccessToken);
        using var nextRefresh = await RefreshAsync(refreshed.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, nextRefresh.StatusCode);
    }

    [Theory]
    [InlineData("not-a-token")]
    [InlineData("%%%")]
    [InlineData("eyJhbGciOiJub25lIn0.e30.")]
    public async Task Refresh_WithMalformedToken_ReturnsUnauthorizedWithoutCredentials(string token)
    {
        using var response = await RefreshAsync(token);

        await FailureAsync(response, HttpStatusCode.Unauthorized, "InvalidRefreshToken", _factory.ApplicationCookieName);
    }

    [Fact]
    public async Task Refresh_WithTamperedToken_ReturnsUnauthorizedWithoutCredentials()
    {
        await _factory.CreateUserAsync();
        var tokens = await LoginTokensAsync(_client);
        // Change decoded ciphertext, avoiding changes to unused base64 padding bits.
        var bytes = WebEncoders.Base64UrlDecode(tokens.RefreshToken);
        bytes[bytes.Length / 2] ^= 1;

        using var response = await RefreshAsync(WebEncoders.Base64UrlEncode(bytes));

        await FailureAsync(response, HttpStatusCode.Unauthorized, "InvalidRefreshToken", _factory.ApplicationCookieName);
    }

    [Fact]
    public async Task Refresh_WithAccessTokenInsteadOfRefreshToken_IsRejected()
    {
        await _factory.CreateUserAsync();
        var tokens = await LoginTokensAsync(_client);

        using var response = await RefreshAsync(tokens.AccessToken);

        await FailureAsync(response, HttpStatusCode.Unauthorized, "InvalidRefreshToken", _factory.ApplicationCookieName);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithRefreshTokenInsteadOfAccessToken_IsRejected()
    {
        await _factory.CreateUserAsync();
        var tokens = await LoginTokensAsync(_client);

        await UnauthorizedAsync(_client, tokens.RefreshToken);
    }

    [Fact]
    public async Task Refresh_AtSevenDayExpiration_IsRejected()
    {
        await _factory.CreateUserAsync();
        var tokens = await LoginTokensAsync(_client);
        _factory.Clock.Advance(TimeSpan.FromDays(7) - TimeSpan.FromSeconds(1));
        using var before = await RefreshAsync(tokens.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, before.StatusCode);

        _factory.Clock.Advance(TimeSpan.FromSeconds(1));
        using var expired = await RefreshAsync(tokens.RefreshToken);

        await FailureAsync(expired, HttpStatusCode.Unauthorized, "InvalidRefreshToken", _factory.ApplicationCookieName);
    }

    [Theory]
    [InlineData(RefreshAccountChange.Deleted)]
    [InlineData(RefreshAccountChange.SecurityStampChanged)]
    [InlineData(RefreshAccountChange.LockedOut)]
    [InlineData(RefreshAccountChange.EmailUnconfirmed)]
    [InlineData(RefreshAccountChange.PhoneConfirmationRequired)]
    public async Task Refresh_WhenAccountCanNoLongerRefresh_IsRejected(RefreshAccountChange change)
    {
        await _factory.CreateUserAsync();
        var tokens = await LoginTokensAsync(_client);
        await _factory.UpdateUserAsync(async (manager, user) =>
        {
            switch (change)
            {
                case RefreshAccountChange.Deleted:
                    AssertSucceeded(await manager.DeleteAsync(user));
                    break;
                case RefreshAccountChange.SecurityStampChanged:
                    AssertSucceeded(await manager.UpdateSecurityStampAsync(user));
                    break;
                case RefreshAccountChange.LockedOut:
                    AssertSucceeded(await manager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15)));
                    break;
                case RefreshAccountChange.EmailUnconfirmed:
                    user.EmailConfirmed = false;
                    AssertSucceeded(await manager.UpdateAsync(user));
                    break;
                case RefreshAccountChange.PhoneConfirmationRequired:
                    // Keep the same host and keys: only change the sign-in requirement.
                    _factory.Services.GetRequiredService<IOptions<IdentityOptions>>().Value
                        .SignIn.RequireConfirmedPhoneNumber = true;
                    break;
            }
        });

        using var response = await RefreshAsync(tokens.RefreshToken);

        await FailureAsync(response, HttpStatusCode.Unauthorized, "InvalidRefreshToken", _factory.ApplicationCookieName);
    }

    [Fact]
    public async Task Refresh_AfterLockoutEnds_CanUseStillValidRefreshToken()
    {
        await _factory.CreateUserAsync();
        var tokens = await LoginTokensAsync(_client);
        await _factory.UpdateUserAsync(async (manager, user) =>
            AssertSucceeded(await manager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15))));
        using var locked = await RefreshAsync(tokens.RefreshToken);
        await FailureAsync(locked, HttpStatusCode.Unauthorized, "InvalidRefreshToken", _factory.ApplicationCookieName);
        await _factory.UpdateUserAsync(async (manager, user) =>
            AssertSucceeded(await manager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(-1))));

        using var response = await RefreshAsync(tokens.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReloginAndRefresh_DoNotRevokeEarlierTokens()
    {
        var user = await _factory.CreateUserAsync();
        var first = await LoginTokensAsync(_client);
        var second = await LoginTokensAsync(_client);

        await AuthenticatedAsync(_client, user.Id, first.AccessToken);
        await AuthenticatedAsync(_client, user.Id, second.AccessToken);
        using var refresh = await RefreshAsync(first.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        await AuthenticatedAsync(_client, user.Id, first.AccessToken);
        using var reuse = await RefreshAsync(first.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, reuse.StatusCode);
    }

    private Task<HttpResponseMessage> RefreshAsync(string token) =>
        _client.PostAsJsonAsync("/api/account/refresh", new RefreshRequest(token));

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
