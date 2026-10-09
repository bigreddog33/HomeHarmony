using Microsoft.AspNetCore.Identity;

namespace Household.Api.Tests.Features.Account.Data;

public static class AccountServiceCases
{
    public sealed class DuplicateErrors : TheoryData<string>
    {
        public DuplicateErrors()
        {
            Add(nameof(IdentityErrorDescriber.DuplicateEmail));
            Add(nameof(IdentityErrorDescriber.DuplicateUserName));
        }
    }

    public sealed class UnsuccessfulConfirmations : TheoryData<SendConfirmationStatus>
    {
        public UnsuccessfulConfirmations()
        {
            Add(SendConfirmationStatus.UserNotFound);
            Add(SendConfirmationStatus.AlreadyConfirmed);
            Add(SendConfirmationStatus.Failed);
        }
    }

    public sealed class ConfirmationTokens : TheoryData<string, SendConfirmationStatus>
    {
        public ConfirmationTokens()
        {
            Add("test-confirmation-token", SendConfirmationStatus.Sent);
            Add("", SendConfirmationStatus.Failed);
        }
    }
}
