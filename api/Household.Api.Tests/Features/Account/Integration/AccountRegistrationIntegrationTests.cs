using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Household.Api.Features.Account.Contracts;
using Household.Api.Tests.Features.Account.Data;
using Household.Api.Tests.Features.Account.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Household.Api.Tests.Features.Account.Data.AccountRegistrationCases;

namespace Household.Api.Tests.Features.Account.Integration;

[Trait("Category", "Integration")]
public sealed class AccountRegistrationIntegrationTests : IDisposable
{
    private readonly AccountApiFactory _factory = new();
    private readonly HttpClient _client;

    public AccountRegistrationIntegrationTests()
    {
        _client = _factory.CreateApiClient();
    }

    [Theory]
    [ClassData(typeof(AccountRegistrationCases.InvalidPasswords))]
    public async Task CreateUser_WithInvalidPassword_RejectsRequestBeforeCallingService(string? password)
    {
        using var response = await _client.PostAsJsonAsync(CreatePath,
            new { email = "user@example.com", password });

        await AssertValidationProblem(response, "Password");
    }

    [Theory]
    [ClassData(typeof(AccountRegistrationCases.ValidPasswords))]
    public async Task CreateUser_WithValidPassword_AcceptsRequest(string password)
    {
        using var response = await _client.PostAsJsonAsync(CreatePath,
            new { email = "user@example.com", password });

        await AssertCreateUserResponse(response, HttpStatusCode.Created, CreateUserStatus.Created);
        var request = Assert.IsType<CreateUserRequest>(
            Assert.Single(_factory.Service.Calls).Request);
        Assert.Equal("user@example.com", request.Email);
        Assert.Equal(password, request.Password);
    }

    [Theory]
    [ClassData(typeof(AccountRegistrationCases.InvalidEmails))]
    public async Task Endpoint_WithInvalidEmail_RejectsRequestBeforeCallingService(string path, string? email)
    {
        using var response = await _client.PostAsJsonAsync(path,
            new { email, password = "Password123!" });

        await AssertValidationProblem(response, "Email");
    }

    [Theory]
    [ClassData(typeof(AccountRegistrationCases.ValidEmails))]
    public async Task Endpoint_WithValidEmail_CallsService(string path, string email)
    {
        using var response = await _client.PostAsJsonAsync(path,
            new { email, password = "Password123!" });

        if (path == CreatePath)
        {
            await AssertCreateUserResponse(response, HttpStatusCode.Created, CreateUserStatus.Created);
        }
        else
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        }
        var request = Assert.Single(_factory.Service.Calls).Request;
        var boundEmail = request switch
        {
            CreateUserRequest create => create.Email,
            ResendConfirmationRequest resend => resend.Email,
            _ => throw new InvalidOperationException("Unexpected service request.")
        };
        Assert.Equal(email, boundEmail);
    }

    [Theory]
    [ClassData(typeof(AccountRegistrationCases.MalformedBodies))]
    public async Task Endpoint_WithMissingOrMalformedBody_RejectsRequestBeforeCallingService(string path, string body)
    {
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(path, content);

        await AssertValidationProblem(response);
    }

    [Theory]
    [ClassData(typeof(AccountRegistrationCases.Endpoints))]
    public async Task Endpoint_WithUnsupportedContentType_ReturnsUnsupportedMediaType(string path)
    {
        using var content = new StringContent("email=user@example.com");
        using var response = await _client.PostAsync(path, content);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.Empty(_factory.Service.Calls);
    }

    [Theory]
    [ClassData(typeof(AccountRegistrationCases.CreateUserResults))]
    public async Task CreateUser_ReturnsServiceStatusAsJson(CreateUserStatus serviceStatus, HttpStatusCode expectedStatus)
    {
        _factory.Service.CreateResult = serviceStatus == CreateUserStatus.CreatedConfirmationFailed
            ? new(serviceStatus, ConfirmationStatus: SendConfirmationStatus.Failed)
            : new(serviceStatus);

        using var response = await _client.PostAsJsonAsync(CreatePath, ValidBody());

        await AssertCreateUserResponse(response, expectedStatus, serviceStatus);
        Assert.Single(_factory.Service.Calls);
    }

    [Fact]
    public async Task CreateUser_WhenServiceRejectsRequest_ReturnsIdentityErrorsAsJson()
    {
        _factory.Service.CreateResult = new(
            CreateUserStatus.ValidationFailed,
            IdentityResult: IdentityResult.Failed(
                new IdentityError
                {
                    Code = "PasswordTooShort",
                    Description = "Password must contain at least 8 characters."
                },
                new IdentityError
                {
                    Code = "PasswordRequiresDigit",
                    Description = "Password must contain at least one digit."
                }));

        // A valid HTTP request reaches the stub, which supplies the Identity failure.
        using var response = await _client.PostAsJsonAsync(CreatePath, ValidBody());

        var body = await AssertCreateUserResponse(
            response, HttpStatusCode.BadRequest, CreateUserStatus.ValidationFailed);
        Assert.Collection(body.GetProperty("errors").EnumerateArray(),
            error =>
            {
                Assert.Equal("PasswordTooShort", error.GetProperty("code").GetString());
                Assert.Equal("Password must contain at least 8 characters.", error.GetProperty("description").GetString());
            },
            error =>
            {
                Assert.Equal("PasswordRequiresDigit", error.GetProperty("code").GetString());
                Assert.Equal("Password must contain at least one digit.", error.GetProperty("description").GetString());
            });
        Assert.Single(_factory.Service.Calls);
    }

    [Theory]
    [ClassData(typeof(AccountRegistrationCases.ResendConfirmationResults))]
    public async Task ResendConfirmation_ReturnsExpectedHttpStatus(
        SendConfirmationStatus serviceStatus, HttpStatusCode expectedStatus)
    {
        _factory.Service.ConfirmationResult = serviceStatus;

        using var response = await _client.PostAsJsonAsync(ResendPath,
            new ResendConfirmationRequest("user@example.com"));

        Assert.Equal(expectedStatus, response.StatusCode);
        var request = Assert.IsType<ResendConfirmationRequest>(
            Assert.Single(_factory.Service.Calls).Request);
        Assert.Equal("user@example.com", request.Email);
        if (expectedStatus == HttpStatusCode.OK)
        {
            Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        }
    }

    [Theory]
    [ClassData(typeof(AccountRegistrationCases.ServiceFailures))]
    public async Task Endpoint_WhenServiceThrows_ReturnsSanitizedServerError(string path, ServiceFailure failureKind)
    {
        const string privateDetail = "Private provider connection details";
        Exception failure = failureKind == ServiceFailure.Timeout
            ? new TimeoutException(privateDetail) : new InvalidOperationException(privateDetail);
        _factory.Service.OnCall = _ => Task.FromException(failure);

        using var response = await _client.PostAsJsonAsync(path, ValidBody());

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain(privateDetail, await response.Content.ReadAsStringAsync());
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Single(_factory.Service.Calls);
    }

    [Theory]
    [ClassData(typeof(AccountRegistrationCases.RequestCancellations))]
    public async Task Endpoint_WhenClientCancelsOrTimesOut_CancelsServiceWork(string path, RequestCancellation cancellation)
    {
        // These signals let us cancel after the service starts and verify that it stops.
        var started = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _factory.Service.OnCall = async token =>
        {
            started.SetResult(token);
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                cancelled.SetResult();
                throw;
            }
        };
        _client.Timeout = cancellation == RequestCancellation.ClientTimeout
            ? TimeSpan.FromSeconds(2) : Timeout.InfiniteTimeSpan;
        // A test safety limit, longer than the client timeout being exercised.
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var pending = _client.PostAsJsonAsync(path, ValidBody(), source.Token);
        var serviceToken = await started.Task.WaitAsync(TimeSpan.FromSeconds(10));

        if (cancellation == RequestCancellation.ClientCancellation)
            source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => pending.WaitAsync(TimeSpan.FromSeconds(10)));
        await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(serviceToken.IsCancellationRequested);
        if (cancellation == RequestCancellation.ClientTimeout)
            Assert.False(source.IsCancellationRequested);
    }

    private static object ValidBody() => new { email = "user@example.com", password = "Password123!" };

    private static async Task<JsonElement> AssertCreateUserResponse(
        HttpResponseMessage response, HttpStatusCode expectedStatus, CreateUserStatus expectedOutcome)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        // Inspect the actual JSON: clients should receive a status name, not an enum number.
        Assert.Equal(expectedOutcome.ToString(), body.GetProperty("status").GetString());
        if (expectedOutcome != CreateUserStatus.ValidationFailed)
        {
            Assert.Equal(JsonValueKind.Null, body.GetProperty("errors").ValueKind);
        }
        return body;
    }

    private async Task AssertValidationProblem(HttpResponseMessage response, string? field = null)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.NotEmpty(problem.Errors);
        if (field is not null)
        {
            Assert.True(problem.Errors.TryGetValue(field, out var errors));
            Assert.NotEmpty(errors);
        }
        Assert.Empty(_factory.Service.Calls);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
