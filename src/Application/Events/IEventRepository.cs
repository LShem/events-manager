using EventsManager.Domain.Events;

namespace EventsManager.Application.Events;

/// <summary>
/// Port de persistance de l'agrégat <see cref="Event"/>.
/// </summary>
public interface IEventRepository
{
    Task AddAsync(Event @event, CancellationToken cancellationToken);

    Task<Event?> GetByIdAsync(EventId id, CancellationToken cancellationToken);
}
