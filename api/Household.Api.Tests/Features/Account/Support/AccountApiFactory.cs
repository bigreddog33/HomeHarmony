using Household.Api.Features.Account.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Household.Api.Tests.Features.Account.Support;

// Runs the real API pipeline in memory, replacing only the account service.
// Each test owns a factory so its service behavior and recorded calls are isolated.
internal sealed class AccountApiFactory : WebApplicationFactory<Program>
{
    public AccountServiceStub Service { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use the real production error handler without requiring Windows Event Log access.
        builder.UseEnvironment("Production");
        builder.ConfigureLogging(logging => logging.ClearProviders().AddConsole());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAccountService>();
            services.AddSingleton<IAccountService>(Service);
        });
    }

    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });
}
