using Microsoft.AspNetCore.Http;

namespace Household.Api.Tests.Features.Account.Data;

public static class AccountControllerCases
{
    public sealed class ResendConfirmationResults : TheoryData<SendConfirmationStatus, int>
    {
        public ResendConfirmationResults()
        {
            Add(SendConfirmationStatus.Sent, StatusCodes.Status200OK);
            Add(SendConfirmationStatus.UserNotFound, StatusCodes.Status404NotFound);
            Add(SendConfirmationStatus.AlreadyConfirmed, StatusCodes.Status200OK);
            Add(SendConfirmationStatus.Failed, StatusCodes.Status500InternalServerError);
        }
    }

    public sealed class CreateUserResults : TheoryData<CreateUserStatus, int>
    {
        public CreateUserResults()
        {
            Add(CreateUserStatus.Created, StatusCodes.Status201Created);
            Add(CreateUserStatus.CreatedConfirmationFailed, StatusCodes.Status201Created);
            Add(CreateUserStatus.AlreadyExists, StatusCodes.Status409Conflict);
            Add(CreateUserStatus.ValidationFailed, StatusCodes.Status400BadRequest);
        }
    }

    public sealed class LoginResults : TheoryData<LoginStatus, int>
    {
        public LoginResults()
        {
            Add(LoginStatus.Succeeded, StatusCodes.Status200OK);
            Add(LoginStatus.InvalidCredentials, StatusCodes.Status401Unauthorized);
            Add(LoginStatus.EmailNotConfirmed, StatusCodes.Status403Forbidden);
            Add(LoginStatus.NotAllowed, StatusCodes.Status403Forbidden);
            Add(LoginStatus.LockedOut, StatusCodes.Status423Locked);
            Add(LoginStatus.RequiresTwoFactor, StatusCodes.Status401Unauthorized);
        }
    }
}
