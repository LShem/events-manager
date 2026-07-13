using EventsManager.Domain.Events;
using EventsManager.Infrastructure.Events;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.IntegrationTests;

/// <summary>
/// Aller-retours réels du repository contre SQL Server : mapping fluent
/// (conversion EventId, nvarchar(100), DateOnly → date) et index unique sur Name.
/// Chaque test écrit puis relit via des DbContext distincts (pas d'illusion de cache),
/// et crée ses propres données avec un nom unique (l'index unique l'exige).
/// </summary>
public class EventRepositoryTests(SqlServerContainerFixture fixture)
{
    private static readonly DateOnly Today = new(2026, 7, 13);
    private static readonly DateOnly ValidDate = new(2026, 12, 31);

    private readonly SqlServerContainerFixture _fixture = fixture;

    [Fact]
    public async Task AddAsync_Then_GetByIdAsync_RoundtripsAggregate()
    {
        var @event = Event.Create(UniqueName("Fête nationale"), ValidDate, Today);
        await AddAsync(@event);

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);
        var reloaded = await repository.GetByIdAsync(@event.Id, TestContext.Current.CancellationToken);

        reloaded.Should().NotBeNull();
        reloaded!.Id.Should().Be(@event.Id);
        reloaded.Name.Should().Be(@event.Name);
        reloaded.Date.Should().Be(@event.Date);
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        var found = await repository.GetByIdAsync(EventId.New(), TestContext.Current.CancellationToken);

        found.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WithDuplicateName_ThrowsDbUpdateException()
    {
        var name = UniqueName("Marché de Noël");
        await AddAsync(Event.Create(name, ValidDate, Today));

        var duplicate = Event.Create(name, ValidDate.AddDays(1), Today);
        Func<Task> act = async () => await AddAsync(duplicate);

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task AddAsync_WithNameOfMaxLength_RoundtripsIntact()
    {
        // 67 « é » + espace + Guid N (32) = 100 caractères exactement : borne nvarchar(100)
        // et Unicode prouvés d'un coup, tout en restant unique.
        var name = $"{new string('é', Event.NameMaxLength - 33)} {Guid.CreateVersion7():N}";
        var @event = Event.Create(name, ValidDate, Today);
        await AddAsync(@event);

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);
        var reloaded = await repository.GetByIdAsync(@event.Id, TestContext.Current.CancellationToken);

        reloaded.Should().NotBeNull();
        reloaded!.Name.Should().Be(name);
    }

    private static string UniqueName(string prefix)
    {
        return $"{prefix} {Guid.CreateVersion7():N}";
    }

    private async Task AddAsync(Event @event)
    {
        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);
        await repository.AddAsync(@event, TestContext.Current.CancellationToken);
    }
}
