using System.ComponentModel.DataAnnotations;
namespace Household.Api.Features.Account.Contracts;

public sealed record LoginRequest(
    [Required, EnumDataType(typeof(LoginMode))] LoginMode? Mode,
    [Required, EmailAddress] string Email,
    [Required] string Password);

public enum LoginMode
{
    Token,
    SessionCookie,
    PersistentCookie
}