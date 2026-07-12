namespace EventsManager.Application.Events;

public sealed record CreateEventCommand(string Name, DateOnly Date);
