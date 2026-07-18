using EventsManager.Application.Events;
using EventsManager.Domain.Events;
using EventsManager.Domain.Orders;
using FluentValidation;
using FluentValidation.Results;

namespace EventsManager.Application.Orders;

/// <summary>
/// Use case : créer une commande pour un évènement existant. CQRS light — handler
/// injecté directement, pas de bus.
/// L'existence de l'évènement se vérifie ici, pas dans le validator (singleton, il
/// ne peut pas dépendre d'un repository scoped) ; la FK physique la garantit aussi
/// en base. La date de création vient du TimeProvider — le domaine ne lit pas
/// l'horloge — et porte le tri « ordre de création » de la liste par évènement.
/// </summary>
public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IEventRepository eventRepository,
    IValidator<CreateOrderCommand> validator,
    TimeProvider timeProvider)
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly IValidator<CreateOrderCommand> _validator = validator;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Guid> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var eventId = EventId.From(command.EventId);

        if (!await _eventRepository.ExistsAsync(eventId, cancellationToken))
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(command.EventId), $"L'évènement « {command.EventId} » n'existe pas.")]);
        }

        var lines = command.Lines
            .Select(line => OrderLine.Create(line.Label, Money.From(line.UnitPrice), line.Quantity))
            .ToList();

        var order = Order.Create(eventId, command.CustomerName, lines, _timeProvider.GetUtcNow().UtcDateTime);
        await _orderRepository.AddAsync(order, cancellationToken);

        return order.Id.Value;
    }
}
