using Household.Api.Features.Account.Contracts;

namespace Household.Api.Features.Account.Services;

public sealed class MockAccountService : IAccountService
{
    public Task<bool> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isValid =
            string.Equals(request.Email, "demo@homeharmony.local", StringComparison.OrdinalIgnoreCase)
            && string.Equals(request.Password, "Password123!", StringComparison.Ordinal);

        return Task.FromResult(isValid);
    }
}
