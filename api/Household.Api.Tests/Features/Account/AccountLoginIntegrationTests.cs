using System.Net;
using System.Net.Http.Json;
using Household.Api.Features.Account.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Household.Api.Tests.Features.Account;

public sealed class AccountLoginIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;

    public AccountLoginIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
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
    [ClassData(typeof(LoginTestCases.InvalidCredentials))]
    public async Task Login_WithValidInputButInvalidCredentials_ReturnsUnauthorized(string email, string password)
    {
        using var response = await _client.PostAsJsonAsync("/api/account/login", new LoginRequest(email, password));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [ClassData(typeof(LoginTestCases.DemoEmails))]
    public async Task Login_WithDemoCredentials_ReturnsOkWithoutResponseBody(string email)
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/account/login", new LoginRequest(email, "Password123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
    }

    private async Task AssertValidationProblemAsync(string? email, string? password, string field)
    {
        using var response = await _client.PostAsJsonAsync("/api/account/login", new { email, password });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.True(problem.Errors.TryGetValue(field, out var errors));
        Assert.NotEmpty(errors);
    }

    public void Dispose() => _client.Dispose();
}
