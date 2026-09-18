using Household.Api.Features.Account.Contracts;

namespace Household.Api.Features.Account.Services;

// Program.cs registers this temporary service for API requests from every client.
// Account persistence and email delivery will be added in the real implementation.
public sealed class MockAccountService : IAccountService
{
    private const string DemoEmail = "demo@homeharmony.local";

    public Task<bool> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isValid =
            string.Equals(request.Email, DemoEmail, StringComparison.OrdinalIgnoreCase)
            && string.Equals(request.Password, "Password123!", StringComparison.Ordinal);

        return Task.FromResult(isValid);
    }

    public Task<bool> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // The API has already validated the request. Skip account creation for now
        // and return the confirmation result: true for the demo email, false otherwise.
        // This does not store the supplied password or change the demo login.
        return SendConfirmationAsync(new ResendConfirmationRequest(request.Email), cancellationToken);
    }

    public Task<bool> SendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Pretend email delivery succeeds only for demo@homeharmony.local.
        // This same check handles both the initial confirmation and resend requests;
        // it does not send an actual email.
        return Task.FromResult(
            string.Equals(request.Email, DemoEmail, StringComparison.OrdinalIgnoreCase));
    }
}
