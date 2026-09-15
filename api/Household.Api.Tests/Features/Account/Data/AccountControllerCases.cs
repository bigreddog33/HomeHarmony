using Microsoft.AspNetCore.Http;

namespace Household.Api.Tests.Features.Account.Data;

public static class AccountControllerCases
{
    public sealed class ServiceResults : TheoryData<bool, int>
    {
        public ServiceResults()
        {
            Add(true, StatusCodes.Status200OK);
            Add(false, StatusCodes.Status401Unauthorized);
        }
    }
}
