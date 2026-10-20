public record LoginResponse(
    LoginStatus Status,
    DateTimeOffset? LockoutEnd = null);