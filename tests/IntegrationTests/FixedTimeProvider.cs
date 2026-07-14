namespace EventsManager.IntegrationTests;

/// <summary>
/// TimeProvider main figé (pas de lib de mock) : instant UTC fixe + fuseau local
/// forcé à UTC, donc GetLocalNow() est déterministe quelle que soit la machine
/// qui exécute les tests. Copie assumée de celui d'UnitTests (internal là-bas :
/// 15 lignes ne justifient pas un projet utilitaire partagé).
/// </summary>
internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    private readonly DateTimeOffset _utcNow = utcNow;

    public override TimeZoneInfo LocalTimeZone
    {
        get { return TimeZoneInfo.Utc; }
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _utcNow;
    }
}
