namespace Household.Api.Tests.Features.Account.Data;

public static class AccountRegistrationCases
{
    public const string CreatePath = "/api/account/createuser";
    public const string ResendPath = "/api/account/resendconfirmation";

    public enum ServiceFailure { Timeout, UnexpectedError }
    public enum RequestCancellation { ClientCancellation, ClientTimeout }

    public sealed class Endpoints : TheoryData<string>
    {
        public Endpoints()
        {
            Add(CreatePath);
            Add(ResendPath);
        }
    }

    public sealed class InvalidEmails : TheoryData<string, string?>
    {
        public InvalidEmails()
        {
            foreach (var path in new Endpoints())
            {
                Add(path, null);
                Add(path, "");
                Add(path, "   ");
                foreach (var email in new LoginTestCases.InvalidEmails())
                    Add(path, email);
            }
        }
    }

    public sealed class ValidEmails : TheoryData<string, string>
    {
        public ValidEmails()
        {
            foreach (var path in new Endpoints())
            {
                Add(path, "user+home@example.com");
                Add(path, "USER@EXAMPLE.COM");
            }
        }
    }

    public sealed class MalformedBodies : TheoryData<string, string>
    {
        public MalformedBodies()
        {
            foreach (var path in new Endpoints())
            {
                Add(path, "");
                Add(path, "null");
                Add(path, "{");
                Add(path, "{}");
                Add(path, "{\"email\":123,\"password\":\"Password123!\"}");
            }
            Add(CreatePath, "{\"email\":\"user@example.com\"}");
            Add(CreatePath, "{\"email\":\"user@example.com\",\"password\":123}");
        }
    }

    public sealed class ServiceFailures : TheoryData<string, ServiceFailure>
    {
        public ServiceFailures()
        {
            foreach (var path in new Endpoints())
            {
                Add(path, ServiceFailure.Timeout);
                Add(path, ServiceFailure.UnexpectedError);
            }
        }
    }

    public sealed class RequestCancellations : TheoryData<string, RequestCancellation>
    {
        public RequestCancellations()
        {
            foreach (var path in new Endpoints())
            {
                Add(path, RequestCancellation.ClientCancellation);
                Add(path, RequestCancellation.ClientTimeout);
            }
        }
    }

    public sealed class InvalidPasswords : TheoryData<string?>
    {
        public InvalidPasswords()
        {
            Add(null);
            Add("");
            Add("        ");
            Add("Aa1!aaa"); // Seven characters; all other requirements satisfied.
            Add("password1!"); // No uppercase letter.
            Add("Password!!"); // No number.
            Add("Password12"); // No special character.
            Add("Password1 "); // Whitespace is not a special character.
            Add("Password1\n");
        }
    }

    public sealed class ValidPasswords : TheoryData<string>
    {
        public ValidPasswords()
        {
            Add("Aa1!aaaa"); // Exactly eight characters.
            Add("Password123!");
            Add("PASSWORD1!"); // Lowercase was not requested.
            Add("Password1_");
            Add("Password1+");
            Add("Password1€");
            Add(new string('a', 128) + "A1!"); // No maximum length is specified.
        }
    }
}
