namespace EventsManager.Application.Events;

public sealed record EventDto(Guid Id, string Name, DateOnly Date);
