
using Microsoft.AspNetCore.Identity;

public record CreateUserResponse(
    CreateUserStatus Status,
    IEnumerable<IdentityError>? Errors = null,
    bool? EmailConfirmed = null);