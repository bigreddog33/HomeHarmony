using Household.Api.Features.Account.Contracts;

namespace Household.Api.Features.Account.Services;

public interface IAccountService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
