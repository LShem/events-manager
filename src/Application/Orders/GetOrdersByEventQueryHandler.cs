using EventsManager.Application.Events;
using EventsManager.Domain.Events;

namespace EventsManager.Application.Orders;

/// <summary>
/// Use case : lister les commandes d'un évènement, en résumé (id, nom du client,
/// total), ordonnées par ordre de création. Retourne null si l'évènement n'existe
/// pas — not found, même convention que <see cref="Events.GetEventQueryHandler"/> —
/// et une liste vide s'il existe sans commandes.
/// </summary>
public sealed class GetOrdersByEventQueryHandler(
    IOrderRepository orderRepository,
    IEventRepository eventRepository)
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IEventRepository _eventRepository = eventRepository;

    public async Task<IReadOnlyList<OrderSummaryDto>?> HandleAsync(
        GetOrdersByEventQuery query,
        CancellationToken cancellationToken)
    {
        var eventId = EventId.From(query.EventId);

        if (!await _eventRepository.ExistsAsync(eventId, cancellationToken))
        {
            return null;
        }

        var orders = await _orderRepository.ListByEventAsync(eventId, cancellationToken);

        return [.. orders.Select(order => order.ToSummaryDto())];
    }
}
