using EventsManager.Domain.Events;

namespace EventsManager.Application.Events;

public sealed record GetEventQuery(EventId Id);
