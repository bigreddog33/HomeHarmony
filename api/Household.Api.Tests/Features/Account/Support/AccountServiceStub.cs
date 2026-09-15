using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Services;

namespace Household.Api.Tests.Features.Account.Support;

// A test-controlled IAccountService. It contains no account or email rules.
// Tests choose the result (or supply asynchronous behavior) and inspect the calls
// to check what the controller forwarded. Only test code registers this service.
internal sealed class AccountServiceStub : IAccountService
{
    public bool Result { get; set; } = true;

    // Used by failure/cancellation tests to throw or wait until the token is cancelled.
    public Func<CancellationToken, Task<bool>>? OnCall { get; set; }

    public List<(object Request, CancellationToken Token)> Calls { get; } = [];

    public Task<bool> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken) =>
        Invoke(request, cancellationToken);

    public Task<bool> SendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken) =>
        Invoke(request, cancellationToken);

    public Task<bool> LoginAsync(LoginRequest request, CancellationToken cancellationToken) =>
        Invoke(request, cancellationToken);

    private Task<bool> Invoke(object request, CancellationToken cancellationToken)
    {
        Calls.Add((request, cancellationToken));
        cancellationToken.ThrowIfCancellationRequested();
        return OnCall?.Invoke(cancellationToken) ?? Task.FromResult(Result);
    }
}
