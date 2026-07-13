using EventsManager.Application.Events;
using EventsManager.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Infrastructure.Events;

/// <summary>
/// Implémentation EF Core du port <see cref="IEventRepository"/>.
/// AddAsync persiste immédiatement : le contrat du port implique la sauvegarde,
/// les handlers n'appellent rien d'autre.
/// </summary>
public sealed class EventRepository(EventsManagerDbContext context) : IEventRepository
{
    private readonly EventsManagerDbContext _context = context;

    public async Task AddAsync(Event @event, CancellationToken cancellationToken)
    {
        _context.Events.Add(@event);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Event?> GetByIdAsync(EventId id, CancellationToken cancellationToken)
    {
        return await _context.Events.SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
