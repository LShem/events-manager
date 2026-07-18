using EventsManager.Application.Orders;
using EventsManager.Domain.Events;
using EventsManager.Domain.Orders;
using EventsManager.UnitTests.Application.Events;

namespace EventsManager.UnitTests.Application.Orders;

public class GetOrdersByEventQueryHandlerTests
{
    private static readonly DateOnly Today = new(2026, 7, 13);
    private static readonly DateTime BaseCreatedAtUtc = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeOrderRepository _orderRepository = new();
    private readonly FakeEventRepository _eventRepository = new();
    private readonly GetOrdersByEventQueryHandler _handler;

    public GetOrdersByEventQueryHandlerTests()
    {
        _handler = new GetOrdersByEventQueryHandler(_orderRepository, _eventRepository);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownEvent_ReturnsNull()
    {
        // OR-R3 : évènement inconnu — not found.
        var knownEvent = AddEvent();
        _orderRepository.Orders.Add(
            Order.Create(knownEvent, "Alice Dupont", [Line("Frites", 4.00m, 2)], BaseCreatedAtUtc));

        var summaries = await _handler.HandleAsync(
            new GetOrdersByEventQuery(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        summaries.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithEventWithoutOrders_ReturnsEmptyList()
    {
        // OR-R3 : évènement existant sans commandes — liste vide, pas not found.
        var eventId = AddEvent();

        var summaries = await _handler.HandleAsync(
            new GetOrdersByEventQuery(eventId.Value),
            TestContext.Current.CancellationToken);

        summaries.Should().NotBeNull();
        summaries!.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithOrdersOfSeveralEvents_ReturnsOnlyThoseOfTheEventOrderedByCreation()
    {
        // OR-R2 : commandes de l'évènement demandé uniquement, en résumé
        // (id, nom du client, total), ordonnées par ordre de création.
        var eventA = AddEvent();
        var eventB = AddEvent();
        var firstOfA = Order.Create(eventA, "Zoé Martin", [Line("Frites", 2.50m, 1)], BaseCreatedAtUtc);
        var secondOfA = Order.Create(eventA, "Anna Petit", [Line("Gaufre", 3.00m, 2)], BaseCreatedAtUtc.AddMinutes(2));
        var ofB = Order.Create(eventB, "Bruno Durand", [Line("Crêpe", 2.00m, 1)], BaseCreatedAtUtc.AddMinutes(1));
        // Ajout volontairement dans le désordre des dates de création : l'ordre du
        // résultat doit suivre l'ordre de création, pas l'ordre d'insertion.
        _orderRepository.Orders.Add(secondOfA);
        _orderRepository.Orders.Add(ofB);
        _orderRepository.Orders.Add(firstOfA);

        var summaries = await _handler.HandleAsync(
            new GetOrdersByEventQuery(eventA.Value),
            TestContext.Current.CancellationToken);

        summaries.Should().NotBeNull();
        summaries!.Should().Equal(
            new OrderSummaryDto(firstOfA.Id.Value, "Zoé Martin", 2.50m),
            new OrderSummaryDto(secondOfA.Id.Value, "Anna Petit", 6.00m));
    }

    [Fact]
    public async Task HandleAsync_WithEmptyEventId_ThrowsArgumentException()
    {
        // Même famille que GetEvent : un Guid vide n'est pas une identité valide.
        Func<Task> act = () => _handler.HandleAsync(
            new GetOrdersByEventQuery(Guid.Empty),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private EventId AddEvent()
    {
        var @event = Event.Create($"Évènement {Guid.CreateVersion7():N}", new DateOnly(2026, 12, 31), Today);
        _eventRepository.Events.Add(@event);

        return @event.Id;
    }

    private static OrderLine Line(string label, decimal unitPrice, int quantity)
    {
        return OrderLine.Create(label, Money.From(unitPrice), quantity);
    }
}
