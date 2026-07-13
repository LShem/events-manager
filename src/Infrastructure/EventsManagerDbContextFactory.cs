using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventsManager.Infrastructure;

/// <summary>
/// Factory design-time pour l'outillage dotnet-ef (migrations add / database update),
/// sans projet de démarrage. Chaîne de connexion de dev uniquement (auth Windows,
/// aucun secret) — le runtime, lui, passe par AddInfrastructure.
/// </summary>
public sealed class EventsManagerDbContextFactory : IDesignTimeDbContextFactory<EventsManagerDbContext>
{
    public EventsManagerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventsManagerDbContext>();
        optionsBuilder.UseEventsManagerSqlServer(
            "Server=localhost;Database=EventsManager;Integrated Security=True;TrustServerCertificate=True");

        return new EventsManagerDbContext(optionsBuilder.Options);
    }
}
