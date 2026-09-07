using Household.Api.Features.Account.Contracts;

namespace Household.Api.Features.Account.Services;

public interface IAccountService
{
    Task<bool> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
