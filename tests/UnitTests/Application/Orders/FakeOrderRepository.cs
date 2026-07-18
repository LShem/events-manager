using EventsManager.Application.Orders;
using EventsManager.Domain.Events;
using EventsManager.Domain.Orders;

namespace EventsManager.UnitTests.Application.Orders;

/// <summary>
/// Double de test main de <see cref="IOrderRepository"/> (pas de lib de mock dans le repo).
/// Honore le contrat du port : ListByEventAsync ne rend que les commandes de
/// l'évènement demandé, ordonnées par ordre de création (CreatedAt).
/// </summary>
internal sealed class FakeOrderRepository : IOrderRepository
{
    public List<Order> Orders { get; } = [];

    public Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Orders.Find(o => o.Id == id));
    }

    public Task<IReadOnlyList<Order>> ListByEventAsync(EventId eventId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Order> result =
        [
            .. Orders
               .Where(o => o.EventId == eventId)
               .OrderBy(o => o.CreatedAt),
        ];

        return Task.FromResult(result);
    }
}
