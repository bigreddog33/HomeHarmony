using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace Household.Api.Tests.Features.Account.Support;

// Controls Identity's answers while tests exercise the real AccountService.
internal sealed class IdentityUserManagerStub : UserManager<ApplicationIdentityUser>
{
    public IdentityUserManagerStub(IServiceProvider services)
        : base(new UnusedUserStore(), Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationIdentityUser>(), [], [],
            new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), services,
            NullLogger<UserManager<ApplicationIdentityUser>>.Instance)
    {
    }

    // Registration looks up the email before creation, then again for confirmation.
    // Queue one answer for each expected lookup; an unexpected extra call fails the test.
    public Queue<ApplicationIdentityUser?> FindResults { get; } = new();
    public IdentityResult CreateResult { get; set; } = IdentityResult.Success;
    public string ConfirmationToken { get; set; } = "test-confirmation-token";

    public List<string> EmailLookups { get; } = [];
    public List<(ApplicationIdentityUser User, string Password)> CreateCalls { get; } = [];
    public List<ApplicationIdentityUser> ConfirmationCalls { get; } = [];

    public override Task<ApplicationIdentityUser?> FindByEmailAsync(string email)
    {
        EmailLookups.Add(email);
        return Task.FromResult(FindResults.Dequeue());
    }

    public override Task<IdentityResult> CreateAsync(ApplicationIdentityUser user, string password)
    {
        CreateCalls.Add((user, password));
        return Task.FromResult(CreateResult);
    }

    public override Task<string> GenerateEmailConfirmationTokenAsync(ApplicationIdentityUser user)
    {
        ConfirmationCalls.Add(user);
        return Task.FromResult(ConfirmationToken);
    }

    // UserManager requires a store, but the overrides above never use one.
    // Throw rather than accidentally pretend that a database operation succeeded.
    private sealed class UnusedUserStore : IUserStore<ApplicationIdentityUser>
    {
        public void Dispose() { }
        public Task<string> GetUserIdAsync(ApplicationIdentityUser user, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<string?> GetUserNameAsync(ApplicationIdentityUser user, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task SetUserNameAsync(ApplicationIdentityUser user, string? userName, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<string?> GetNormalizedUserNameAsync(ApplicationIdentityUser user, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task SetNormalizedUserNameAsync(ApplicationIdentityUser user, string? normalizedName, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<IdentityResult> CreateAsync(ApplicationIdentityUser user, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<IdentityResult> UpdateAsync(ApplicationIdentityUser user, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<IdentityResult> DeleteAsync(ApplicationIdentityUser user, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<ApplicationIdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task<ApplicationIdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
