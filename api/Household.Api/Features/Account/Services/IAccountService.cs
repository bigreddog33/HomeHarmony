using Household.Api.Features.Account.Contracts;

namespace Household.Api.Features.Account.Services;

public interface IAccountService
{
    Task<CreateUserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<SendConfirmationStatus> SendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken);
    Task LogoutAsync(CancellationToken cancellationToken);
    Task<RefreshStatus> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken);
}
