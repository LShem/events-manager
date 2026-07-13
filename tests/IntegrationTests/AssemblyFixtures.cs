using EventsManager.IntegrationTests;

// Fixtures d'assembly xUnit v3 : un seul conteneur SQL Server pour toute l'assembly,
// injecté dans les constructeurs des classes de test.
[assembly: AssemblyFixture(typeof(SqlServerContainerFixture))]
