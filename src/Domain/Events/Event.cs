namespace EventsManager.Domain.Events;

/// <summary>
/// Racine d'agrégat : un évènement géré par le back-office.
/// Invariants et préconditions garantis ici, indépendamment de toute validation amont :
/// nom non vide (ni blanc) et limité à <see cref="NameMaxLength"/> caractères après trim ;
/// à la création, date au moins à J+1 — le domaine ne lit pas l'horloge, « aujourd'hui »
/// est fourni par l'appelant.
/// </summary>
public sealed class Event
{
    public const int NameMaxLength = 100;

    public EventId Id { get; }

    public string Name { get; }

    public DateOnly Date { get; }

    private Event(EventId id, string name, DateOnly date)
    {
        Id = id;
        Name = name;
        Date = date;
    }

    public static Event Create(string name, DateOnly date, DateOnly today)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmedName = name.Trim();

        if (trimmedName.Length > NameMaxLength)
        {
            throw new ArgumentException(
                $"Le nom de l'évènement ne peut pas dépasser {NameMaxLength} caractères (reçu : {trimmedName.Length}).",
                nameof(name));
        }

        if (date <= today)
        {
            throw new ArgumentOutOfRangeException(
                nameof(date),
                $"La date de l'évènement doit être au moins à J+1 (date reçue : {date:o}, aujourd'hui : {today:o}).");
        }

        return new Event(EventId.New(), trimmedName, date);
    }
}
