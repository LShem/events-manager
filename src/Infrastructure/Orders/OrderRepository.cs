using EventsManager.Application.Orders;
using EventsManager.Domain.Events;
using EventsManager.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Infrastructure.Orders;

/// <summary>
/// Implémentation EF Core du port <see cref="IOrderRepository"/>.
/// AddAsync persiste immédiatement, racine et lignes dans le même SaveChanges
/// (transaction implicite) : la commande est créée complète et atomique.
/// Les lignes, owned, se chargent toujours avec la racine — aucun Include requis.
/// </summary>
public sealed class OrderRepository(EventsManagerDbContext context) : IOrderRepository
{
    private readonly EventsManagerDbContext _context = context;

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken)
    {
        return await _context.Orders.SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> ListByEventAsync(EventId eventId, CancellationToken cancellationToken)
    {
        // Le tri « ordre de création » est porté par CreatedAt ; le ThenBy sur l'ID
        // n'apporte que le déterminisme en cas d'égalité à la milliseconde.
        return await _context.Orders
                             .Where(o => o.EventId == eventId)
                             .OrderBy(o => o.CreatedAt)
                             .ThenBy(o => o.Id)
                             .ToListAsync(cancellationToken);
    }
}
