using EventsManager.Domain.Events;
using EventsManager.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Infrastructure.Orders;

/// <summary>
/// Configuration fluent de l'agrégat <see cref="Order"/> → tables app.Orders et
/// app.OrderLines (lignes owned : chargées systématiquement avec la racine,
/// persistées avec elle dans le même SaveChanges — la commande est atomique).
/// Les colonnes d'audit des deux tables (AddedBy, AddedDate, UpdatedBy, UpdatedDate)
/// n'existent volontairement pas dans ce modèle, même pas en shadow : créées par la
/// migration Orders et alimentées côté SQL (DEFAULT + trigger), elles restent
/// invisibles d'EF (cf. <c>EventConfiguration</c>).
/// </summary>
internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // HasTrigger obligatoire : depuis EF Core 7, SaveChanges utilise une clause OUTPUT
        // sans INTO, que SQL Server refuse sur une table portant un trigger. Déclarer le
        // trigger fait basculer EF sur la stratégie compatible.
        builder.ToTable("Orders", table => table.HasTrigger("TR_Orders_Update_Audit"));

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
               .HasConversion(id => id.Value, value => OrderId.From(value))
               .ValueGeneratedNever(); // UUIDv7 généré par le domaine, jamais par la DB

        builder.Property(o => o.EventId)
               .HasConversion(id => id.Value, value => EventId.From(value));

        // FK physique vers app.Events, sans navigation (référence inter-agrégats par
        // identité). Restrict : aucun use case de suppression ; un cascade silencieux
        // supprimerait des commandes.
        builder.HasOne<Event>()
               .WithMany()
               .HasForeignKey(o => o.EventId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

        builder.Property(o => o.CustomerName)
               .HasMaxLength(Order.CustomerNameMaxLength); // nvarchar(100) ; NOT NULL par convention NRT

        // Le tri métier « ordre de création » de la liste par évènement est porté par
        // CreatedAt (uniqueidentifier trie par les 6 derniers octets, jamais par l'ID).
        builder.HasIndex(o => new { o.EventId, o.CreatedAt });

        // Total : calculé par le domaine à partir des lignes, ni saisi ni stocké.
        builder.Ignore(o => o.Total);

        builder.OwnsMany(o => o.Lines, lines =>
        {
            lines.ToTable("OrderLines", table => table.HasTrigger("TR_OrderLines_Update_Audit"));

            lines.WithOwner().HasForeignKey("OrderId");

            // PK composite (OrderId, Label) : aucune génération côté DB, et unicité
            // physique des libellés par commande — la collation CI par défaut de
            // SQL Server la rend insensible à la casse, comme la règle du domaine.
            lines.HasKey("OrderId", nameof(OrderLine.Label));

            lines.Property(l => l.Label)
                 .HasMaxLength(OrderLine.LabelMaxLength);

            lines.Property(l => l.UnitPrice)
                 .HasConversion(money => money.Amount, value => Money.From(value))
                 .HasPrecision(9, Money.MaxDecimalPlaces); // precision 9 : jusqu'à 9 999 999,99 € l'unité, large pour des commandes d'évènement

            // Déclaration explicite requise : une auto-propriété get-only n'est pas
            // découverte par convention, et le binding par constructeur exige que
            // chaque paramètre corresponde à une propriété mappée.
            lines.Property(l => l.Quantity);

            // Sous-total : calculé par le domaine, jamais stocké.
            lines.Ignore(l => l.Subtotal);
        });
    }
}
