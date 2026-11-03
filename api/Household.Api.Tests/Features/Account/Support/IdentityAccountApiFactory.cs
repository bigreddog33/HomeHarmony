using System.Net;
using System.Net.Http.Json;
using Household.Api.Features.Account.Contracts;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;

namespace Household.Api.Tests.Features.Account.Support;

// Real AccountService, Identity managers, EF store, and cookie/bearer handlers.
// Only storage, the clock, and Data Protection key persistence are replaced.
internal sealed class IdentityAccountApiFactory : WebApplicationFactory<Program>
{
    public const string Email = "user@example.com";
    public const string Password = "Password123!";
    public const string ProtectedPath = "/test/authentication";

    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    public TestTimeProvider Clock { get; } = new();

    public IdentityAccountApiFactory()
    {
        _connection.Open();
    }

    public string ApplicationCookieName => Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
        .Get(IdentityConstants.ApplicationScheme).Cookie.Name!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureLogging(logging => logging.ClearProviders().AddConsole()
            .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)
            .AddFilter("Microsoft.AspNetCore", LogLevel.Warning));
        builder.ConfigureServices(services =>
        {
            // Remove the SQL Server options callback as well as its context options.
            services.RemoveAll<IDbContextOptionsConfiguration<UserIdentityDbContext>>();
            services.RemoveAll<DbContextOptions<UserIdentityDbContext>>();
            services.RemoveAll<UserIdentityDbContext>();
            services.AddDbContext<UserIdentityDbContext>(options => options.UseSqlite(_connection));

            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
            services.PostConfigure<BearerTokenOptions>(IdentityConstants.BearerScheme,
                options => options.TimeProvider = Clock);
            services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme,
                options => options.TimeProvider = Clock);

            services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new TestKeyRepository();
                options.XmlEncryptor = new NullXmlEncryptor();
            });
            services.AddControllers().AddApplicationPart(typeof(AuthenticationProbeController).Assembly);
        });
    }

    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    public async Task<ApplicationIdentityUser> CreateUserAsync(bool emailConfirmed = true)
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<UserIdentityDbContext>().Database.EnsureCreatedAsync();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationIdentityUser>>();
        var user = new ApplicationIdentityUser
        {
            UserName = Email,
            Email = Email,
            EmailConfirmed = emailConfirmed
        };
        AssertSucceeded(await manager.CreateAsync(user, Password));
        return user;
    }

    public async Task UpdateUserAsync(Func<UserManager<ApplicationIdentityUser>, ApplicationIdentityUser, Task> update)
    {
        using var scope = Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationIdentityUser>>();
        var user = await manager.FindByEmailAsync(Email);
        Assert.NotNull(user);
        await update(manager, user);
    }

    public async Task<ApplicationIdentityUser> ReadUserAsync()
    {
        using var scope = Services.CreateScope();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationIdentityUser>>()
            .FindByEmailAsync(Email);
        Assert.NotNull(user);
        return user;
    }

    public static Task<HttpResponseMessage> LoginAsync(HttpClient client, LoginMode mode,
        string password = Password, string email = Email) =>
        client.PostAsJsonAsync("/api/account/login", new LoginRequest(mode, email, password));

    public static async Task<AccessTokenResponse> LoginTokensAsync(HttpClient client)
    {
        using var response = await LoginAsync(client, LoginMode.Token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tokens = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
        return tokens;
    }

    public static void AssertSucceeded(IdentityResult result) =>
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}

internal sealed class TestTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow = DateTimeOffset.UtcNow;
    public override DateTimeOffset GetUtcNow() => _utcNow;
    public void Advance(TimeSpan duration) => _utcNow += duration;
}
