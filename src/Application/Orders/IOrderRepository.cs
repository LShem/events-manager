using EventsManager.Domain.Events;
using EventsManager.Domain.Orders;

namespace EventsManager.Application.Orders;

/// <summary>
/// Port de persistance de l'agrégat <see cref="Order"/>.
/// </summary>
public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken);

    /// <summary>Commandes de l'évènement uniquement, ordonnées par ordre de création.</summary>
    Task<IReadOnlyList<Order>> ListByEventAsync(EventId eventId, CancellationToken cancellationToken);
}
