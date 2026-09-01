using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Household.Api.Features.Account.Contracts;

namespace Household.Api.Tests.Features.Account;

public sealed class LoginRequestValidationTests
{
    [Theory]
    [MemberData(nameof(LoginTestCases.InvalidRequests), MemberType = typeof(LoginTestCases))]
    public void LoginRequest_WithMissingInput_IsInvalid(string email, string password)
    {
        var request = new LoginRequest(email, password);

        var isValid = IsValid(request);

        Assert.False(isValid);
    }

    [Theory]
    [InlineData("user@example.com", "x")]
    [InlineData("not-an-email", "password")]
    public void LoginRequest_WithNonEmptyInput_IsValid(string email, string password)
    {
        var request = new LoginRequest(email, password);

        var isValid = IsValid(request);

        Assert.True(isValid);
    }

    private static bool IsValid(LoginRequest request)
    {
        var constructorParameters = typeof(LoginRequest)
            .GetConstructors()
            .Single()
            .GetParameters();

        return constructorParameters.All(parameter =>
        {
            var value = typeof(LoginRequest)
                .GetProperty(parameter.Name!)!
                .GetValue(request);
            var validationAttributes = parameter.GetCustomAttributes<ValidationAttribute>();

            return Validator.TryValidateValue(
                value,
                new ValidationContext(request),
                validationResults: null,
                validationAttributes);
        });
    }
}
