using Microsoft.AspNetCore.Identity;

public record CreateUserResult(
    CreateUserStatus Status,
    IdentityResult? IdentityResult = null,
    SendConfirmationStatus? ConfirmationStatus = null,
    bool? EmailConfirmed = null);