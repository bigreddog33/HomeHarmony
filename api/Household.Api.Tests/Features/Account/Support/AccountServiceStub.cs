using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Services;

namespace Household.Api.Tests.Features.Account.Support;

internal sealed class AccountServiceStub : IAccountService
{
    public bool LoginResult { get; set; } = true;
    public CreateUserResult CreateResult { get; set; } = new (CreateUserStatus.Created);
    public SendConfirmationStatus ConfirmationResult { get; set; } = SendConfirmationStatus.Sent;

    public Func<CancellationToken, Task>? OnCall { get; set; }
    public List<(object Request, CancellationToken Token)> Calls { get; } = [];

    public async Task<CreateUserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        await RecordCallAsync(request, cancellationToken);
        return CreateResult;
    }

    public async Task<SendConfirmationStatus> SendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        await RecordCallAsync(request, cancellationToken);
        return ConfirmationResult;
    }

    public async Task<bool> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        await RecordCallAsync(request, cancellationToken);
        return LoginResult;
    }

    private async Task RecordCallAsync(object request, CancellationToken cancellationToken)
    {
        Calls.Add((request, cancellationToken));
        cancellationToken.ThrowIfCancellationRequested();

        if (OnCall is not null)
        {
            await OnCall(cancellationToken);
        }
    }
}
