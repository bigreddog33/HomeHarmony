using System.Net;
using System.Net.Http.Json;
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
        _factory.Service.Result = false;
        using var response = await _client.PostAsJsonAsync("/api/account/login", new LoginRequest(email, password));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Single(_factory.Service.Calls);
    }

    [Theory]
    [ClassData(typeof(LoginTestCases.ValidRequests))]
    public async Task Login_WhenServiceAcceptsCredentials_ReturnsOkWithoutResponseBody(string email, string password)
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/account/login", new LoginRequest(email, password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        var request = Assert.IsType<LoginRequest>(Assert.Single(_factory.Service.Calls).Request);
        Assert.Equal(email, request.Email);
        Assert.Equal(password, request.Password);
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
        Assert.Empty(_factory.Service.Calls);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
