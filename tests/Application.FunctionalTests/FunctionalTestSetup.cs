using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Application.FunctionalTests;

[SetUpFixture]
public class FunctionalTestSetup
{
    // Assumes `docker compose up -d` has already started Postgres (see docker-compose.yml).
    private const string DefaultConnectionString =
        "Server=127.0.0.1;Port=5432;Database=CleanArchitectureDb;Username=admin;Password=password;";

    internal static IServiceScopeFactory ScopeFactory { get; private set; } = null!;
    internal static DatabaseResetter? DbResetter { get; private set; }

    private static WebApiFactory? _factory;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CleanArchitectureDb")
            ?? DefaultConnectionString;

        _factory = new WebApiFactory(connectionString);
        ScopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        DbResetter = await DatabaseResetter.CreateAsync(connectionString);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (DbResetter is not null) await DbResetter.DisposeAsync();
        if (_factory is not null) await _factory.DisposeAsync();
    }
}
