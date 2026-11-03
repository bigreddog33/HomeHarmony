using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Household.Api.Features.Account.Contracts;
using Household.Api.Tests.Features.Account.Data;
using Household.Api.Tests.Features.Account.Support;
using Microsoft.AspNetCore.Mvc;

namespace Household.Api.Tests.Features.Account.Integration;

[Trait("Category", "Integration")]
public sealed class AccountLoginIntegrationTests : IDisposable
{
    private readonly AccountApiFactory _factory = new();
    private readonly HttpClient _client;

    public AccountLoginIntegrationTests()
    {
        _client = _factory.CreateApiClient();
    }

    [Theory]
    [ClassData(typeof(LoginTestCases.InvalidRequests))]
    [ClassData(typeof(LoginTestCases.NullRequests))]
    public async Task Login_WithMissingInput_ReturnsValidationProblem(string? email, string? password)
    {
        await AssertValidationProblemAsync(email, password,
            string.IsNullOrWhiteSpace(email) ? "Email" : "Password");
    }

    [Theory]
    [ClassData(typeof(LoginTestCases.InvalidEmails))]
    public async Task Login_WithInvalidEmail_ReturnsValidationProblem(string email)
    {
        await AssertValidationProblemAsync(email, "password", "Email");
    }

    [Theory]
    [ClassData(typeof(LoginTestCases.ValidRequests))]
    public async Task Login_WhenServiceRejectsCredentials_ReturnsUnauthorized(string email, string password)
    {
        _factory.Service.LoginResult = new(LoginStatus.InvalidCredentials);
        using var response = await _client.PostAsJsonAsync("/api/account/login",
            new LoginRequest(LoginMode.SessionCookie, email, password));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("InvalidCredentials", body.GetProperty("status").GetString());
        Assert.Single(_factory.Service.Calls);
    }

    [Theory]
    [ClassData(typeof(LoginTestCases.ValidRequests))]
    public async Task Login_WhenServiceAcceptsCredentials_ReturnsSuccessAsJson(string email, string password)
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/account/login", new LoginRequest(LoginMode.SessionCookie, email, password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Succeeded", body.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("lockoutEnd").ValueKind);
        var request = Assert.IsType<LoginRequest>(Assert.Single(_factory.Service.Calls).Request);
        Assert.Equal(email, request.Email);
        Assert.Equal(password, request.Password);
        Assert.Equal(LoginMode.SessionCookie, request.Mode);
    }

    [Theory]
    [ClassData(typeof(LoginTestCases.InvalidModes))]
    public async Task Login_WithMissingOrInvalidMode_DoesNotCallService(string body)
    {
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync("/api/account/login", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.NotEmpty(problem.Errors);
        Assert.Empty(_factory.Service.Calls);
    }

    [Theory]
    [ClassData(typeof(AccountControllerCases.LoginResults))]
    public async Task Login_SerializesEachOutcomeAndLockoutDeadline(LoginStatus outcome, int statusCode)
    {
        var deadline = outcome == LoginStatus.LockedOut
            ? new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.Zero) : (DateTimeOffset?)null;
        _factory.Service.LoginResult = new(outcome, deadline);

        using var response = await _client.PostAsJsonAsync("/api/account/login",
            new LoginRequest(LoginMode.SessionCookie, "user@example.com", "Password123!"));

        Assert.Equal(statusCode, (int)response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(outcome.ToString(), body.GetProperty("status").GetString());
        if (deadline.HasValue)
            Assert.Equal(deadline.Value, body.GetProperty("lockoutEnd").GetDateTimeOffset());
        else
            Assert.Equal(JsonValueKind.Null, body.GetProperty("lockoutEnd").ValueKind);
    }

    private async Task AssertValidationProblemAsync(string? email, string? password, string field)
    {
        using var response = await _client.PostAsJsonAsync("/api/account/login",
            new { email, password, mode = "SessionCookie" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.True(problem.Errors.TryGetValue(field, out var errors));
        Assert.NotEmpty(errors);
        Assert.Empty(_factory.Service.Calls);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
