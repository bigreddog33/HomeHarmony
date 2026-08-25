using System.ComponentModel.DataAnnotations;

namespace Household.Api.Features.Account.Contracts;

public sealed record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);
