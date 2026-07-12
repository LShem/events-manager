using EventsManager.Domain.Events;

namespace EventsManager.Application.Events;

public static class EventMappings
{
    public static EventDto ToDto(this Event @event)
    {
        return new EventDto(@event.Id.Value, @event.Name, @event.Date);
    }
}
