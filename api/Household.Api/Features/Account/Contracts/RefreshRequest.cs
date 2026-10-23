using System.ComponentModel.DataAnnotations;

public sealed record RefreshRequest(
    [Required] string RefreshToken);