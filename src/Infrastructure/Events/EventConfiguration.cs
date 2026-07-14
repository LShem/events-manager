using EventsManager.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Infrastructure.Events;

/// <summary>
/// Configuration fluent de l'agrégat <see cref="Event"/> → table app.Events.
/// EF matérialise l'agrégat par son constructeur privé (liaison par nom de paramètre).
/// Les colonnes d'audit (AddedBy, AddedDate, UpdatedBy, UpdatedDate) n'existent
/// volontairement pas dans ce modèle, même pas en shadow : créées par la migration
/// EventsAudit et alimentées côté SQL (DEFAULT + trigger), elles restent invisibles
/// d'EF — ses INSERT ne les mentionnent jamais et le snapshot ne les connaît pas.
/// </summary>
internal sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        // HasTrigger obligatoire : depuis EF Core 7, SaveChanges utilise une clause OUTPUT
        // sans INTO, que SQL Server refuse sur une table portant un trigger. Déclarer le
        // trigger fait basculer EF sur la stratégie compatible.
        builder.ToTable("Events", table => table.HasTrigger("TR_Events_Update_Audit"));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .HasConversion(id => id.Value, value => EventId.From(value))
               .ValueGeneratedNever(); // UUIDv7 généré par le domaine, jamais par la DB

        builder.Property(e => e.Name)
               .HasMaxLength(Event.NameMaxLength); // nvarchar(100) ; NOT NULL par convention NRT

        // Date : DateOnly → date, mapping natif SQL Server.
        // Unicité métier : un évènement (par nom) n'a lieu qu'une fois par année civile.
        // Year est calculée par SQL Server (persistée), en shadow : dérivée de Date,
        // jamais écrite par le code.
        builder.Property<int>("Year").HasComputedColumnSql("YEAR([Date])", stored: true);
        builder.HasIndex("Name", "Year").IsUnique();

        builder.HasIndex(e => e.Date); // le tri chronologique métier est porté par Date
    }
}
