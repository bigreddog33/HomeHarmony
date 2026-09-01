using System.ComponentModel.DataAnnotations;

namespace Household.Api.Features.Account.Contracts;

public sealed record LoginRequest(
    [Required] string Email,
    [Required] string Password);