using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Services;

namespace Household.Api.Tests.Features.Account.Support;

internal sealed class AccountServiceStub : IAccountService
{
    public LoginResult LoginResult { get; set; } = new(LoginStatus.Succeeded);
    public RefreshStatus RefreshResult { get; set; } = RefreshStatus.Succeeded;
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

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        await RecordCallAsync(request, cancellationToken);
        return LoginResult;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        await RecordCallAsync(new LogoutRequest(), cancellationToken);
    }

    public async Task<RefreshStatus> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        await RecordCallAsync(request, cancellationToken);
        return RefreshResult;
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
