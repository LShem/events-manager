using Microsoft.EntityFrameworkCore;

namespace EventsManager.Infrastructure;

/// <summary>
/// Point unique de configuration SQL Server du DbContext (partagé entre le câblage DI,
/// la factory design-time et les tests d'intégration) : la table d'historique des
/// migrations est rangée explicitement dans le schéma « app », comme le reste du modèle.
/// </summary>
public static class EventsManagerDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseEventsManagerSqlServer(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString)
    {
        return optionsBuilder.UseSqlServer(
            connectionString,
            sqlServer => sqlServer.MigrationsHistoryTable("__EFMigrationsHistory", "app"));
    }
}
