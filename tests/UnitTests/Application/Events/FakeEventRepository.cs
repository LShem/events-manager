using EventsManager.Application.Events;
using EventsManager.Domain.Events;

namespace EventsManager.UnitTests.Application.Events;

/// <summary>
/// Double de test main de <see cref="IEventRepository"/> (pas de lib de mock dans le repo).
/// </summary>
internal sealed class FakeEventRepository : IEventRepository
{
    public List<Event> Events { get; } = [];

    public Task AddAsync(Event @event, CancellationToken cancellationToken)
    {
        Events.Add(@event);
        return Task.CompletedTask;
    }

    public Task<Event?> GetByIdAsync(EventId id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Events.Find(e => e.Id == id));
    }
}
