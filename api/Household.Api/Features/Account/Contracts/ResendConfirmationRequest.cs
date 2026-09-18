using System.ComponentModel.DataAnnotations;

namespace Household.Api.Features.Account.Contracts;

public sealed record ResendConfirmationRequest(
    [Required, EmailAddress] string Email
);
