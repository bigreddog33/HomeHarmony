public sealed record LoginResult(
    LoginStatus Status,
    DateTimeOffset? LockoutEnd = null);