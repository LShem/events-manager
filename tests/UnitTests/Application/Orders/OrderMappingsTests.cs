using EventsManager.Application.Orders;
using EventsManager.Domain.Events;
using EventsManager.Domain.Orders;

namespace EventsManager.UnitTests.Application.Orders;

public class OrderMappingsTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ToDto_Maps_AllProperties()
    {
        // OR-R1 : la commande complète — lignes et total (2 × 4.00 + 3 × 1.50 = 12.50).
        var order = Order.Create(
            EventId.New(),
            "Alice Dupont",
            [Line("Frites", 4.00m, 2), Line("Bière", 1.50m, 3)],
            CreatedAtUtc);

        var dto = order.ToDto();

        dto.Id.Should().Be(order.Id.Value);
        dto.EventId.Should().Be(order.EventId.Value);
        dto.CustomerName.Should().Be("Alice Dupont");
        dto.Lines.Should().BeEquivalentTo(new[]
        {
            new OrderLineDto("Frites", 4.00m, 2, 8.00m),
            new OrderLineDto("Bière", 1.50m, 3, 4.50m),
        });
        dto.Total.Should().Be(12.50m);
    }

    [Fact]
    public void ToDto_OnLine_MapsSnapshotAndSubtotal()
    {
        // OR-L4 : le sous-total exposé est prix unitaire × quantité (4.00 × 2 = 8.00).
        var line = OrderLine.Create("Frites", Money.From(4.00m), 2);

        var dto = line.ToDto();

        dto.Should().Be(new OrderLineDto("Frites", 4.00m, 2, 8.00m));
    }

    [Fact]
    public void ToSummaryDto_Maps_IdCustomerNameAndTotal()
    {
        // OR-R2 : le résumé porte id, nom du client et total.
        var order = Order.Create(
            EventId.New(),
            "Alice Dupont",
            [Line("Frites", 4.00m, 2), Line("Bière", 1.50m, 3)],
            CreatedAtUtc);

        var summary = order.ToSummaryDto();

        summary.Should().Be(new OrderSummaryDto(order.Id.Value, "Alice Dupont", 12.50m));
    }

    private static OrderLine Line(string label, decimal unitPrice, int quantity)
    {
        return OrderLine.Create(label, Money.From(unitPrice), quantity);
    }
}
