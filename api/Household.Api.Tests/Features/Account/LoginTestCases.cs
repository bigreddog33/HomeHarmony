namespace Household.Api.Tests.Features.Account;

public static class LoginTestCases
{
    public static TheoryData<string, string> InvalidCredentials => new()
    {
        { "someone@example.com", "Password123!" },
        { "demo@homeharmony.local", "wrong-password" },
        { "someone@example.com", "wrong-password" }
    };

    public static TheoryData<string, string> InvalidRequests => new()
    {
        { "", "password" },
        { "   ", "password" },
        { "not-an-email", "password" },
        { "user@example.com", "" },
        { "user@example.com", "   " }
    };
}