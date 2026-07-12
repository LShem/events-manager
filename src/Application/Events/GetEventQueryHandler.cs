using EventsManager.Domain.Events;

namespace EventsManager.Application.Events;

/// <summary>
/// Use case : lire un évènement par son identité. Retourne null si introuvable
/// (la traduction en 404 appartient à la couche au-dessus).
/// L'identifiant arrive en <see cref="Guid"/> brut : la conversion via
/// <see cref="EventId.From"/> laisse le domaine intercepter les valeurs invalides.
/// </summary>
public sealed class GetEventQueryHandler(IEventRepository eventRepository)
{
    private readonly IEventRepository _eventRepository = eventRepository;

    public async Task<EventDto?> HandleAsync(GetEventQuery query, CancellationToken cancellationToken)
    {
        var id = EventId.From(query.Id);
        var @event = await _eventRepository.GetByIdAsync(id, cancellationToken);

        return @event?.ToDto();
    }
}
