using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Household.Api.Features.Account.Contracts;
using Household.Api.Tests.Features.Account.Data;
using Household.Api.Tests.Features.Account.Support;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using static Household.Api.Tests.Features.Account.Support.AuthenticationAssertions;
using static Household.Api.Tests.Features.Account.Support.IdentityAccountApiFactory;

namespace Household.Api.Tests.Features.Account.Integration;

[Trait("Category", "Integration")]
public sealed class AccountAuthenticationIntegrationTests : IDisposable
{
    private readonly IdentityAccountApiFactory _factory = new();
    private readonly HttpClient _client;

    public AccountAuthenticationIntegrationTests()
    {
        _client = _factory.CreateApiClient();
    }

    [Theory]
    [InlineData(LoginMode.SessionCookie)]
    [InlineData(LoginMode.PersistentCookie)]
    public async Task Login_WithCookieMode_IssuesUsableCookieWithRequestedPersistence(LoginMode mode)
    {
        var user = await _factory.CreateUserAsync();
        await UnauthorizedAsync(_client);

        using var response = await LoginAsync(_client, mode);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Succeeded", body.GetProperty("status").GetString());
        Assert.False(body.TryGetProperty("accessToken", out _));
        Assert.False(body.TryGetProperty("refreshToken", out _));
        var cookie = Assert.Single(Cookies(response), cookie => cookie.Name.Value == _factory.ApplicationCookieName);
        Assert.True(cookie.HttpOnly);
        Assert.True(cookie.Secure);
        Assert.Equal(Microsoft.Net.Http.Headers.SameSiteMode.Lax, cookie.SameSite);
        if (mode == LoginMode.PersistentCookie)
        {
            Assert.NotNull(cookie.Expires);
            Assert.InRange(cookie.Expires.Value,
                _factory.Clock.GetUtcNow().AddDays(7).AddSeconds(-1), _factory.Clock.GetUtcNow().AddDays(7));
        }
        else
        {
            Assert.Null(cookie.Expires);
            Assert.Null(cookie.MaxAge);
        }
        await AuthenticatedAsync(_client, user.Id);
    }

    [Fact]
    public async Task Login_WithTokenMode_IssuesUsableTokensWithoutApplicationCookie()
    {
        var user = await _factory.CreateUserAsync();

        using var response = await LoginAsync(_client, LoginMode.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var tokens = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        Assert.NotNull(tokens);
        Assert.Equal("Bearer", tokens.TokenType);
        Assert.Equal(900, tokens.ExpiresIn);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
        Assert.NotEqual(tokens.AccessToken, tokens.RefreshToken);
        Assert.DoesNotContain(Cookies(response), cookie => cookie.Name.Value == _factory.ApplicationCookieName);
        await UnauthorizedAsync(_client); // A token response must not also create a browser session.
        await AuthenticatedAsync(_client, user.Id, tokens.AccessToken);
    }

    [Theory]
    [ClassData(typeof(AccountAuthenticationCases.InvalidCredentials))]
    public async Task Login_WithInvalidCredentials_DoesNotIssueCredentials(
        LoginMode mode, bool emailConfirmed, bool unknownEmail)
    {
        await _factory.CreateUserAsync(emailConfirmed);

        using var response = await LoginAsync(_client, mode,
            password: unknownEmail ? Password : "WrongPassword123!",
            email: unknownEmail ? "missing@example.com" : Email);

        await FailureAsync(response, HttpStatusCode.Unauthorized, "InvalidCredentials", _factory.ApplicationCookieName);
        await UnauthorizedAsync(_client);
    }

    [Theory]
    [ClassData(typeof(AccountAuthenticationCases.LoginModes))]
    public async Task Login_WithCorrectPasswordAndUnconfirmedEmail_DoesNotIssueCredentials(LoginMode mode)
    {
        await _factory.CreateUserAsync(emailConfirmed: false);

        using var response = await LoginAsync(_client, mode);

        await FailureAsync(response, HttpStatusCode.Forbidden, "EmailNotConfirmed", _factory.ApplicationCookieName);
        Assert.Equal(0, (await _factory.ReadUserAsync()).AccessFailedCount);
        await UnauthorizedAsync(_client);
    }

    [Fact]
    public async Task Login_WithAnotherConfirmationRequirement_ReturnsNotAllowedOnlyForCorrectPassword()
    {
        await _factory.CreateUserAsync();
        using var configuredFactory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.Configure<IdentityOptions>(options =>
                options.SignIn.RequireConfirmedPhoneNumber = true)));
        using var client = configuredFactory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false
        });

        using var wrongPassword = await LoginAsync(client, LoginMode.Token, "WrongPassword123!");
        await FailureAsync(wrongPassword, HttpStatusCode.Unauthorized, "InvalidCredentials", _factory.ApplicationCookieName);
        using var correctPassword = await LoginAsync(client, LoginMode.Token);
        await FailureAsync(correctPassword, HttpStatusCode.Forbidden, "NotAllowed", _factory.ApplicationCookieName);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Login_FifthFailureLocksAccount_AndCorrectPasswordCannotBypassLockout(bool emailConfirmed)
    {
        await _factory.CreateUserAsync(emailConfirmed);
        var started = DateTimeOffset.UtcNow;

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            using var response = await LoginAsync(_client, LoginMode.Token, "WrongPassword123!");
            var body = await FailureAsync(response,
                attempt == 5 ? HttpStatusCode.Locked : HttpStatusCode.Unauthorized,
                attempt == 5 ? "LockedOut" : "InvalidCredentials", _factory.ApplicationCookieName);
            if (attempt == 5)
            {
                var lockoutEnd = body.GetProperty("lockoutEnd").GetDateTimeOffset();
                Assert.InRange(lockoutEnd, started.AddMinutes(15), DateTimeOffset.UtcNow.AddMinutes(15));
                Assert.Equal((await _factory.ReadUserAsync()).LockoutEnd, lockoutEnd);
            }
            else
            {
                Assert.Equal(attempt, (await _factory.ReadUserAsync()).AccessFailedCount);
            }
        }

        foreach (var mode in Enum.GetValues<LoginMode>())
        {
            using var response = await LoginAsync(_client, mode);
            await FailureAsync(response, HttpStatusCode.Locked, "LockedOut", _factory.ApplicationCookieName);
        }
        await UnauthorizedAsync(_client);
    }

    [Fact]
    public async Task Login_AfterLockoutExpires_Succeeds()
    {
        var user = await _factory.CreateUserAsync();
        await _factory.UpdateUserAsync(async (manager, account) =>
            AssertSucceeded(await manager.SetLockoutEndDateAsync(account, DateTimeOffset.UtcNow.AddMinutes(15))));
        using var locked = await LoginAsync(_client, LoginMode.Token);
        await FailureAsync(locked, HttpStatusCode.Locked, "LockedOut", _factory.ApplicationCookieName);

        // UserManager uses UTC wall time for lockout. Set the persisted deadline in the past
        // instead of sleeping or replacing Identity's lockout implementation.
        await _factory.UpdateUserAsync(async (manager, account) =>
            AssertSucceeded(await manager.SetLockoutEndDateAsync(account, DateTimeOffset.UtcNow.AddMinutes(-1))));

        var tokens = await LoginTokensAsync(_client);
        Assert.Equal(0, (await _factory.ReadUserAsync()).AccessFailedCount);
        await AuthenticatedAsync(_client, user.Id, tokens.AccessToken);
    }

    [Fact]
    public async Task Login_SuccessResetsPreviousFailures()
    {
        await _factory.CreateUserAsync();
        for (var attempt = 0; attempt < 4; attempt++)
        {
            using var failure = await LoginAsync(_client, LoginMode.Token, "WrongPassword123!");
            await FailureAsync(failure, HttpStatusCode.Unauthorized, "InvalidCredentials", _factory.ApplicationCookieName);
        }
        Assert.Equal(4, (await _factory.ReadUserAsync()).AccessFailedCount);

        await LoginTokensAsync(_client);

        Assert.Equal(0, (await _factory.ReadUserAsync()).AccessFailedCount);
        using var nextFailure = await LoginAsync(_client, LoginMode.Token, "WrongPassword123!");
        await FailureAsync(nextFailure, HttpStatusCode.Unauthorized, "InvalidCredentials", _factory.ApplicationCookieName);
        Assert.Equal(1, (await _factory.ReadUserAsync()).AccessFailedCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Login_WhenLockoutIsDisabled_DoesNotLockAccount(bool emailConfirmed)
    {
        await _factory.CreateUserAsync(emailConfirmed);
        await _factory.UpdateUserAsync(async (manager, user) =>
            AssertSucceeded(await manager.SetLockoutEnabledAsync(user, false)));

        for (var attempt = 0; attempt < 6; attempt++)
        {
            using var failure = await LoginAsync(_client, LoginMode.Token, "WrongPassword123!");
            await FailureAsync(failure, HttpStatusCode.Unauthorized, "InvalidCredentials", _factory.ApplicationCookieName);
        }

        using var response = await LoginAsync(_client, LoginMode.Token);
        if (emailConfirmed)
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        else
            await FailureAsync(response, HttpStatusCode.Forbidden, "EmailNotConfirmed", _factory.ApplicationCookieName);
    }

    [Theory]
    [InlineData(LoginMode.SessionCookie)]
    [InlineData(LoginMode.PersistentCookie)]
    public async Task Cookie_ExpiresAfterConfiguredLifetimeWithoutSlidingRenewal(LoginMode mode)
    {
        var user = await _factory.CreateUserAsync();
        using var login = await LoginAsync(_client, mode);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        _factory.Clock.Advance(TimeSpan.FromDays(4));
        await AuthenticatedAsync(_client, user.Id);
        _factory.Clock.Advance(TimeSpan.FromDays(3) + TimeSpan.FromSeconds(1));

        await UnauthorizedAsync(_client);
    }

    [Fact]
    public async Task AccessToken_ExpiresAtConfiguredLifetime()
    {
        var user = await _factory.CreateUserAsync();
        var tokens = await LoginTokensAsync(_client);
        _factory.Clock.Advance(TimeSpan.FromSeconds(tokens.ExpiresIn - 1));
        await AuthenticatedAsync(_client, user.Id, tokens.AccessToken);

        _factory.Clock.Advance(TimeSpan.FromSeconds(1));

        await UnauthorizedAsync(_client, tokens.AccessToken);
    }

    [Fact]
    public async Task FailedLogin_PreservesExistingBrowserSession()
    {
        var user = await _factory.CreateUserAsync();
        using var login = await LoginAsync(_client, LoginMode.SessionCookie);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var failure = await LoginAsync(_client, LoginMode.SessionCookie, "WrongPassword123!");

        await FailureAsync(failure, HttpStatusCode.Unauthorized, "InvalidCredentials", _factory.ApplicationCookieName);
        await AuthenticatedAsync(_client, user.Id);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
