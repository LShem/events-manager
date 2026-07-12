namespace EventsManager.Application;

/// <summary>
/// Point de vérité unique de la définition métier d'« aujourd'hui » : la date locale
/// du serveur. Simplification assumée tant que l'hébergement reste dans le fuseau
/// des évènements ; si ça change, c'est ici (et uniquement ici) qu'on branche un
/// fuseau explicite.
/// </summary>
public static class TimeProviderExtensions
{
    public static DateOnly TodayLocal(this TimeProvider timeProvider)
    {
        return DateOnly.FromDateTime(timeProvider.GetLocalNow().Date);
    }
}
