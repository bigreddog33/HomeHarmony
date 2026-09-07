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
    [InlineData("not-an-email")]
    [InlineData("user.example.com")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@@example.com")]
    public void LoginRequest_WithInvalidEmail_IsInvalid(string email)
    {
        var request = new LoginRequest(email, "password");

        var isValid = IsValid(request);

        Assert.False(isValid);
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("user@example.com", null)]
    public void LoginRequest_WithNullInput_IsInvalid(string? email, string? password)
    {
        var request = new LoginRequest(email!, password!);

        var isValid = IsValid(request);

        Assert.False(isValid);
    }

    [Theory]
    [InlineData("user@example.com", "x")]
    [InlineData("demo@homeharmony.local", "password")]
    [InlineData("user+home@example.com", "123")]
    public void LoginRequest_WithValidEmailAndNonEmptyPassword_IsValid(string email, string password)
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
