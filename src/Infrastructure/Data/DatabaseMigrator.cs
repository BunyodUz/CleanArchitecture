using DbUp;
using DbUp.Postgresql;

namespace CleanArchitecture.Infrastructure.Data;

public static class DatabaseMigrator
{
    public static void MigrateDatabase(string connectionString)
    {
        var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Data", "Scripts");

        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsFromFileSystem(scriptsPath)
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw result.Error;
        }
    }
}
