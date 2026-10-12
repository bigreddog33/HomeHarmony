using Household.Api.Features.Account.Contracts;

namespace Household.Api.Features.Account.Services;

public interface IAccountService
{
    Task<CreateUserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<bool> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<SendConfirmationStatus> SendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken);
}
