using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanArchitecture.Web.AcceptanceTests;

// Boots the Web app on a real Kestrel socket (rather than the in-memory TestServer
// WebApplicationFactory uses by default) so Playwright has a real URL to navigate to.
public class AcceptanceTestHost(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:CleanArchitectureDb", connectionString);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

        var host = builder.Build();
        host.Start();

        var addresses = host.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>();

        ServerAddress = addresses!.Addresses.First();

        return host;
    }

    public string ServerAddress { get; private set; } = null!;
}

[SetUpFixture]
public class AcceptanceTestSetup
{
    // Assumes `docker compose up -d` has already started Postgres (see docker-compose.yml).
    private const string DefaultConnectionString =
        "Server=127.0.0.1;Port=5432;Database=CleanArchitectureDb;Username=admin;Password=password;";

    public static AcceptanceTestHost Host { get; private set; } = null!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CleanArchitectureDb")
            ?? DefaultConnectionString;

        Host = new AcceptanceTestHost(connectionString);

        // Touch Server to force the host (and the real Kestrel listener) to start.
        _ = Host.Server;
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Host.DisposeAsync();
    }
}
