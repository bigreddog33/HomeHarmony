namespace Household.Api.Tests.Features.Account;

public static class LoginTestCases
{
    public sealed class InvalidCredentials : TheoryData<string, string>
    {
        public InvalidCredentials()
        {
            Add("someone@example.com", "Password123!");
            Add("demo@homeharmony.local", "wrong-password");
            Add("demo@homeharmony.local", "password123!");
            Add("someone@example.com", "wrong-password");
        }
    }

    public sealed class InvalidRequests : TheoryData<string, string>
    {
        public InvalidRequests()
        {
            Add("", "password");
            Add("   ", "password");
            Add("user@example.com", "");
            Add("user@example.com", "   ");
        }
    }

    public sealed class InvalidEmails : TheoryData<string>
    {
        public InvalidEmails()
        {
            Add("not-an-email");
            Add("user.example.com");
            Add("@example.com");
            Add("user@");
            Add("user@@example.com");
        }
    }

    public sealed class NullRequests : TheoryData<string?, string?>
    {
        public NullRequests()
        {
            Add(null, "password");
            Add("user@example.com", null);
        }
    }

    public sealed class ValidRequests : TheoryData<string, string>
    {
        public ValidRequests()
        {
            Add("user@example.com", "x");
            Add("demo@homeharmony.local", "password");
            Add("user+home@example.com", "123");
        }
    }

    public sealed class DemoEmails : TheoryData<string>
    {
        public DemoEmails()
        {
            Add("demo@homeharmony.local");
            Add("DEMO@HOMEHARMONY.LOCAL");
        }
    }
}
