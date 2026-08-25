using System.ComponentModel.DataAnnotations;
using Household.Api.Features.Account.Contracts;

namespace Household.Api.Tests.Features.Account;

public sealed class LoginRequestValidationTests
{
    [Theory]
    [MemberData(nameof(LoginTestCases.InvalidRequests), MemberType = typeof(LoginTestCases))]
    public void LoginRequest_WithMissingOrMalformedInput_IsInvalid(string email, string password)
    {
        var request = new LoginRequest(email, password);

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults: null,
            validateAllProperties: true);

        Assert.False(isValid);
    }

    [Fact]
    public void LoginRequest_WithShortNonEmptyPassword_IsValid()
    {
        var request = new LoginRequest("user@example.com", "x");

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults: null,
            validateAllProperties: true);

        Assert.True(isValid);
    }
}
