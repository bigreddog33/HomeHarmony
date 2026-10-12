using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Controllers;
using Household.Api.Tests.Features.Account.Data;
using Household.Api.Tests.Features.Account.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Household.Api.Tests.Features.Account.Unit;

[Trait("Category", "Unit")]
public sealed class AccountCreateUserControllerTests
{
    [Theory]
    [ClassData(typeof(AccountControllerCases.CreateUserResults))]
    public async Task CreateUser_ForwardsRequestAndToken_AndReturnsServiceStatus(CreateUserStatus serviceStatus, int expectedStatus)
    {
        // Arrange: choose what the service returns, independently of any account rules.
        var serviceResult = serviceStatus switch
        {
            CreateUserStatus.ValidationFailed => new CreateUserResult(
                serviceStatus,
                IdentityResult: IdentityResult.Failed(new IdentityError
                {
                    Code = "PasswordTooShort",
                    Description = "Password must contain at least 8 characters."
                })),
            CreateUserStatus.CreatedConfirmationFailed => new CreateUserResult(
                serviceStatus,
                ConfirmationStatus: SendConfirmationStatus.Failed),
            _ => new CreateUserResult(serviceStatus)
        };
        var service = new AccountServiceStub { CreateResult = serviceResult };
        var controller = new AccountController(service);
        var request = new CreateUserRequest("user@example.com", "Password123!");
        using var source = new CancellationTokenSource();

        // Act: call the controller directly. No HTTP server is involved.
        var result = await controller.CreateUser(request, source.Token);

        // Assert: the controller forwards the request/token and maps the service outcome.
        var call = Assert.Single(service.Calls);
        Assert.Same(request, call.Request);
        Assert.Equal(source.Token, call.Token);
        var response = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(expectedStatus, response.StatusCode);
        var body = Assert.IsType<CreateUserResponse>(response.Value);
        Assert.Equal(serviceStatus, body.Status);

        if (serviceStatus != CreateUserStatus.ValidationFailed)
        {
            Assert.Null(body.Errors);
        }
    }

    [Fact]
    public async Task CreateUser_WhenValidationFails_ReturnsAllIdentityErrors()
    {
        var identityResult = IdentityResult.Failed(
            new IdentityError
            {
                Code = "PasswordTooShort",
                Description = "Password must contain at least 8 characters."
            },
            new IdentityError
            {
                Code = "PasswordRequiresDigit",
                Description = "Password must contain at least one digit."
            });
        var service = new AccountServiceStub
        {
            CreateResult = new(CreateUserStatus.ValidationFailed, IdentityResult: identityResult)
        };
        var controller = new AccountController(service);
        var request = new CreateUserRequest("user@example.com", "Password123!");

        var result = await controller.CreateUser(request, CancellationToken.None);

        var response = Assert.IsType<BadRequestObjectResult>(result);
        var body = Assert.IsType<CreateUserResponse>(response.Value);
        Assert.Equal(CreateUserStatus.ValidationFailed, body.Status);
        Assert.NotNull(body.Errors);
        Assert.Collection(body.Errors,
            error =>
            {
                Assert.Equal("PasswordTooShort", error.Code);
                Assert.Equal("Password must contain at least 8 characters.", error.Description);
            },
            error =>
            {
                Assert.Equal("PasswordRequiresDigit", error.Code);
                Assert.Equal("Password must contain at least one digit.", error.Description);
            });
    }

    [Fact]
    public async Task CreateUser_WithCancelledToken_PropagatesCancellation()
    {
        var service = new AccountServiceStub();
        var controller = new AccountController(service);
        var request = new CreateUserRequest("user@example.com", "Password123!");
        using var source = new CancellationTokenSource();
        source.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => controller.CreateUser(request, source.Token));

        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.Equal(source.Token, Assert.Single(service.Calls).Token);
    }
}
