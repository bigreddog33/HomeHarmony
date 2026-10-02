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

    public sealed class ServiceResults : TheoryData<bool, int>
    {
        public ServiceResults()
        {
            Add(true, StatusCodes.Status200OK);
            Add(false, StatusCodes.Status401Unauthorized);
        }
    }
}
