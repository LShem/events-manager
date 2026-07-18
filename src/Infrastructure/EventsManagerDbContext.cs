using EventsManager.Domain.Events;
using EventsManager.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Infrastructure;

/// <summary>
/// DbContext EF Core de l'application. Tout le modèle vit dans le schéma « app » ;
/// la configuration de chaque agrégat est portée par une IEntityTypeConfiguration dédiée
/// (découverte par assembly).
/// </summary>
public sealed class EventsManagerDbContext(DbContextOptions<EventsManagerDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("app");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventsManagerDbContext).Assembly);
    }
}
