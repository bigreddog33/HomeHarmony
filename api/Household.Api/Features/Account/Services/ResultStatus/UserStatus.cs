using Microsoft.AspNetCore.Identity;

public enum CreateUserStatus
{
    Created,
    CreatedConfirmationFailed,
    AlreadyExists,
    ValidationFailed
}

public enum SendConfirmationStatus
{
    Sent,
    UserNotFound,
    AlreadyConfirmed,
    Failed
}

public record CreateUserResult(
    CreateUserStatus Status,
    IdentityResult? IdentityResult = null,
    SendConfirmationStatus? ConfirmationStatus = null,
    bool? EmailConfirmed = null);

public record CreateUserResponse(
    CreateUserStatus Status,
    IEnumerable<IdentityError>? Errors = null,
    bool? EmailConfirmed = null);