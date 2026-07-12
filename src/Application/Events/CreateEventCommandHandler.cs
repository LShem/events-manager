using EventsManager.Domain.Events;
using FluentValidation;

namespace EventsManager.Application.Events;

/// <summary>
/// Use case : créer un évènement. CQRS light — handler injecté directement, pas de bus.
/// </summary>
public sealed class CreateEventCommandHandler(
    IEventRepository eventRepository,
    IValidator<CreateEventCommand> validator,
    TimeProvider timeProvider)
{
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly IValidator<CreateEventCommand> _validator = validator;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Guid> HandleAsync(CreateEventCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var today = _timeProvider.TodayLocal();
        var @event = Event.Create(command.Name, command.Date, today);
        await _eventRepository.AddAsync(@event, cancellationToken);

        return @event.Id.Value;
    }
}
