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