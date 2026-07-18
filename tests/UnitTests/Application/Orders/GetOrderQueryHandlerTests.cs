using EventsManager.Application.Orders;
using EventsManager.Domain.Events;
using EventsManager.Domain.Orders;

namespace EventsManager.UnitTests.Application.Orders;

public class GetOrderQueryHandlerTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeOrderRepository _repository = new();
    private readonly GetOrderQueryHandler _handler;

    public GetOrderQueryHandlerTests()
    {
        _handler = new GetOrderQueryHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_WithExistingOrder_ReturnsItsDtoWithLinesAndTotal()
    {
        // OR-R1 : la commande est retournée avec ses lignes et son total
        // (2 × 4.00 + 3 × 1.50 = 12.50).
        var order = Order.Create(
            EventId.New(),
            "Alice Dupont",
            [Line("Frites", 4.00m, 2), Line("Bière", 1.50m, 3)],
            CreatedAtUtc);
        _repository.Orders.Add(order);

        var dto = await _handler.HandleAsync(new GetOrderQuery(order.Id.Value), TestContext.Current.CancellationToken);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(order.Id.Value);
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
    public async Task HandleAsync_WithSeveralOrders_ReturnsOnlyTheTargetedOne()
    {
        var first = Order.Create(EventId.New(), "Alice Dupont", [Line("Frites", 4.00m, 2)], CreatedAtUtc);
        var target = Order.Create(EventId.New(), "Bruno Petit", [Line("Bière", 1.50m, 3)], CreatedAtUtc);
        var third = Order.Create(EventId.New(), "Chloé Martin", [Line("Gaufre", 3.00m, 1)], CreatedAtUtc);
        _repository.Orders.Add(first);
        _repository.Orders.Add(target);
        _repository.Orders.Add(third);

        var dto = await _handler.HandleAsync(new GetOrderQuery(target.Id.Value), TestContext.Current.CancellationToken);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(target.Id.Value);
        dto.CustomerName.Should().Be("Bruno Petit");
    }

    [Fact]
    public async Task HandleAsync_WithUnknownId_ReturnsNull()
    {
        // OR-R1 : id inconnu — not found (même comportement que GetEvent).
        _repository.Orders.Add(
            Order.Create(EventId.New(), "Alice Dupont", [Line("Frites", 4.00m, 2)], CreatedAtUtc));

        var dto = await _handler.HandleAsync(new GetOrderQuery(Guid.CreateVersion7()), TestContext.Current.CancellationToken);

        dto.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Même famille que GetEvent : un Guid vide n'est pas une identité valide.
        Func<Task> act = () => _handler.HandleAsync(new GetOrderQuery(Guid.Empty), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private static OrderLine Line(string label, decimal unitPrice, int quantity)
    {
        return OrderLine.Create(label, Money.From(unitPrice), quantity);
    }
}
