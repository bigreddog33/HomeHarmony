using Household.Api.Features.Account.Contracts;

namespace Household.Api.Tests.Features.Account.Data;

public static class AccountAuthenticationCases
{
    public sealed class LoginModes : TheoryData<LoginMode>
    {
        public LoginModes()
        {
            Add(LoginMode.SessionCookie);
            Add(LoginMode.PersistentCookie);
            Add(LoginMode.Token);
        }
    }

    public sealed class InvalidCredentials : TheoryData<LoginMode, bool, bool>
    {
        public InvalidCredentials()
        {
            foreach (var mode in Enum.GetValues<LoginMode>())
            {
                Add(mode, true, false); // Confirmed account, wrong password.
                Add(mode, false, false); // Unconfirmed account, wrong password.
                Add(mode, true, true); // Unknown email.
            }
        }
    }

    public sealed class MalformedBodies : TheoryData<string, string>
    {
        public MalformedBodies()
        {
            foreach (var path in new[] { "/api/account/login", "/api/account/logout", "/api/account/refresh" })
            {
                Add(path, "");
                Add(path, "null");
                Add(path, "{");
                Add(path, "[]");
            }
        }
    }

    public sealed class MissingRefreshTokens : TheoryData<string>
    {
        public MissingRefreshTokens()
        {
            Add("{}");
            Add("{\"refreshToken\":null}");
            Add("{\"refreshToken\":\"\"}");
            Add("{\"refreshToken\":\"   \"}");
            Add("{\"refreshToken\":123}");
        }
    }

    public enum RefreshAccountChange
    {
        Deleted,
        SecurityStampChanged,
        LockedOut,
        EmailUnconfirmed,
        PhoneConfirmationRequired
    }
}
