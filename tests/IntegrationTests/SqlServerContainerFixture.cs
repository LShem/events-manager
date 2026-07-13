using EventsManager.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace EventsManager.IntegrationTests;

/// <summary>
/// Un conteneur SQL Server (Testcontainers) partagé par tous les tests d'intégration,
/// migré une seule fois par les migrations EF réelles — la fixture prouve donc aussi
/// InitialCreate et EventsAudit (schéma app, colonnes d'audit, trigger) sur un vrai SQL Server.
/// Prérequis : Docker actif.
/// </summary>
public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    // Image explicite : le constructeur sans paramètre de MsSqlBuilder est obsolète (CS0618).
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public EventsManagerDbContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventsManagerDbContext>();
        optionsBuilder.UseEventsManagerSqlServer(_container.GetConnectionString());

        return new EventsManagerDbContext(optionsBuilder.Options);
    }
}
