using Household.Api.Features.Account.Contracts;

namespace Household.Api.Features.Account.Services;

public sealed class MockAccountService : IAccountService
{
    public Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LoginResponse? response =
            string.Equals(request.Email, "demo@homeharmony.local", StringComparison.OrdinalIgnoreCase)
            && string.Equals(request.Password, "Password123!", StringComparison.Ordinal) 
                ? new LoginResponse()
                : null;

        return Task.FromResult(response);
    }
}
