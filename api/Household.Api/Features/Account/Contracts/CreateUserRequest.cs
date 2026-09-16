using System.ComponentModel.DataAnnotations;

namespace Household.Api.Features.Account.Contracts;

public sealed record CreateUserRequest(
    [Required, EmailAddress] string EmailAddress,
    [Required, MinLength(8)]
    [RegularExpression(@"(?s)\A(?=.*[0-9])(?=.*[A-Z])(?=.*[\p{P}\p{S}]).*\z",
        ErrorMessage = "Password must contain at least one number, one uppercase letter, and one special character.")]
    string Password
);
