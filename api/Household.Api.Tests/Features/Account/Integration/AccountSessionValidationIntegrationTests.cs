using System.Net;
using System.Net.Http.Json;
using System.Text;
using Household.Api.Tests.Features.Account.Data;
using Household.Api.Tests.Features.Account.Support;
using Microsoft.AspNetCore.Mvc;

namespace Household.Api.Tests.Features.Account.Integration;

// The stub makes it observable that invalid HTTP input never reaches account logic.
[Trait("Category", "Integration")]
public sealed class AccountSessionValidationIntegrationTests : IDisposable
{
    private readonly AccountApiFactory _factory = new();
    private readonly HttpClient _client;

    public AccountSessionValidationIntegrationTests()
    {
        _client = _factory.CreateApiClient();
    }

    [Theory]
    [ClassData(typeof(AccountAuthenticationCases.MalformedBodies))]
    public async Task Endpoint_WithMissingOrMalformedBody_RejectsBeforeService(string path, string body)
    {
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(path, content);

        await AssertValidationProblemAsync(response);
    }

    [Theory]
    [ClassData(typeof(AccountAuthenticationCases.MissingRefreshTokens))]
    public async Task Refresh_WithMissingOrInvalidTokenField_RejectsBeforeService(string body)
    {
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync("/api/account/refresh", content);

        await AssertValidationProblemAsync(response);
    }

    [Theory]
    [InlineData("/api/account/login")]
    [InlineData("/api/account/logout")]
    [InlineData("/api/account/refresh")]
    public async Task Endpoint_WithFormBody_ReturnsUnsupportedMediaType(string path)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["email"] = "user@example.com", ["password"] = "Password123!", ["refreshToken"] = "token"
        });
        using var response = await _client.PostAsync(path, content);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.Empty(_factory.Service.Calls);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Theory]
    [InlineData("/api/account/login", "{\"mode\":\"Token\",\"email\":\"user@example.com\",\"password\":\"Password123!\"}")]
    [InlineData("/api/account/logout", "{}")]
    [InlineData("/api/account/refresh", "{\"refreshToken\":\"token\"}")]
    public async Task Endpoint_WhenServiceThrows_ReturnsSanitizedServerError(string path, string body)
    {
        const string privateDetail = "Private authentication storage details";
        _factory.Service.OnCall = _ => Task.FromException(new InvalidOperationException(privateDetail));
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        using var response = await _client.PostAsync(path, content);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain(privateDetail, await response.Content.ReadAsStringAsync());
        Assert.False(response.Headers.Contains("Set-Cookie"));
        Assert.Single(_factory.Service.Calls);
    }

    private async Task AssertValidationProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.NotEmpty(problem.Errors);
        Assert.Empty(_factory.Service.Calls);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
