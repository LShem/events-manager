namespace EventsManager.Application.Events;

/// <summary>
/// Use case : lire un évènement par son identité. Retourne null si introuvable
/// (la traduction en 404 appartient à la couche au-dessus).
/// </summary>
public sealed class GetEventQueryHandler(IEventRepository eventRepository)
{
    private readonly IEventRepository _eventRepository = eventRepository;

    public async Task<EventDto?> HandleAsync(GetEventQuery query, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(query.Id, cancellationToken);

        return @event?.ToDto();
    }
}
