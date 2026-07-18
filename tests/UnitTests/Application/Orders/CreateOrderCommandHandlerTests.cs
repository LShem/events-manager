using EventsManager.Application.Orders;
using EventsManager.Domain.Events;
using EventsManager.UnitTests.Application.Events;
using FluentValidation;

namespace EventsManager.UnitTests.Application.Orders;

public class CreateOrderCommandHandlerTests
{
    private static readonly DateOnly Today = new(2026, 7, 13);
    private static readonly DateTimeOffset FixedUtcNow = new(2026, 7, 13, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeOrderRepository _orderRepository = new();
    private readonly FakeEventRepository _eventRepository = new();

    public static readonly TheoryData<DateTimeOffset> SubmissionInstants = new()
    {
        new DateTimeOffset(2026, 7, 13, 12, 0, 0, TimeSpan.Zero), // veille de l'évènement
        new DateTimeOffset(2026, 7, 14, 12, 0, 0, TimeSpan.Zero), // jour même
        new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero),  // après l'évènement
    };

    [Fact]
    public async Task HandleAsync_WithValidCommand_PersistsOrderAndReturnsItsId()
    {
        var eventId = AddEvent(new DateOnly(2026, 12, 31));
        var command = new CreateOrderCommand(
            eventId,
            "Alice Dupont",
            [new("Frites", 4.00m, 2), new("Bière", 1.50m, 3)]);

        var id = await CreateHandler(FixedUtcNow).HandleAsync(command, TestContext.Current.CancellationToken);

        var order = _orderRepository.Orders.Should().ContainSingle().Subject;
        order.Id.Value.Should().Be(id);
        order.EventId.Value.Should().Be(eventId);
        order.CustomerName.Should().Be("Alice Dupont");
        // OR-R2 : « ordre de création » — la date de création reflète l'instant de la saisie.
        order.CreatedAt.Should().Be(FixedUtcNow.UtcDateTime);
        // OR-C6 : la commande est créée complète — la racine et toutes ses lignes.
        order.Lines.Should().HaveCount(2);
        // OR-C5 : 2 × 4.00 + 3 × 1.50 = 12.50.
        order.Total.Amount.Should().Be(12.50m);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownEvent_ThrowsAndDoesNotPersist()
    {
        // OR-C1 : création vers un évènement inconnu — rejetée, rien n'est persisté.
        AddEvent(new DateOnly(2026, 12, 31));
        var command = new CreateOrderCommand(
            Guid.CreateVersion7(),
            "Alice Dupont",
            [new("Frites", 4.50m, 2)]);

        Func<Task> act = () => CreateHandler(FixedUtcNow).HandleAsync(command, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ValidationException>();
        _orderRepository.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCustomerName_ThrowsAndDoesNotPersist()
    {
        // OR-C2 + OR-P3 (versant applicatif) : commande invalide — rien n'est persisté.
        var eventId = AddEvent(new DateOnly(2026, 12, 31));
        var command = new CreateOrderCommand(eventId, "   ", [new("Frites", 4.50m, 2)]);

        Func<Task> act = () => CreateHandler(FixedUtcNow).HandleAsync(command, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ValidationException>();
        _orderRepository.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateLabels_ThrowsAndDoesNotPersist()
    {
        // OR-C4 + OR-P3 (versant applicatif) : doublon de libellés — rien n'est persisté.
        var eventId = AddEvent(new DateOnly(2026, 12, 31));
        var command = new CreateOrderCommand(
            eventId,
            "Alice Dupont",
            [new("Frites", 4.00m, 2), new("frites", 1.50m, 3)]);

        Func<Task> act = () => CreateHandler(FixedUtcNow).HandleAsync(command, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ValidationException>();
        _orderRepository.Orders.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(SubmissionInstants))]
    public async Task HandleAsync_AtAnyInstantAroundEventDate_Succeeds(DateTimeOffset submittedAt)
    {
        // OR-C8 : aucune contrainte entre le moment de la saisie et la date de
        // l'évènement (14/07/2026) — avant, jour même, après : tout est accepté.
        var eventId = AddEvent(new DateOnly(2026, 7, 14));
        var command = new CreateOrderCommand(eventId, "Alice Dupont", [new("Frites", 4.50m, 2)]);

        await CreateHandler(submittedAt).HandleAsync(command, TestContext.Current.CancellationToken);

        var order = _orderRepository.Orders.Should().ContainSingle().Subject;
        order.CreatedAt.Should().Be(submittedAt.UtcDateTime);
    }

    private CreateOrderCommandHandler CreateHandler(DateTimeOffset utcNow)
    {
        return new CreateOrderCommandHandler(
            _orderRepository,
            _eventRepository,
            new CreateOrderCommandValidator(),
            new FixedTimeProvider(utcNow));
    }

    private Guid AddEvent(DateOnly date)
    {
        var @event = Event.Create($"Évènement {Guid.CreateVersion7():N}", date, Today);
        _eventRepository.Events.Add(@event);

        return @event.Id.Value;
    }
}
