using EventsManager.Application.Events;
using EventsManager.Domain.Events;

namespace EventsManager.UnitTests.Application.Events;

public class GetEventQueryHandlerTests
{
    private static readonly DateOnly Today = new(2026, 7, 13);
    private static readonly DateOnly ValidDate = new(2026, 7, 14);

    private readonly FakeEventRepository _repository = new();
    private readonly GetEventQueryHandler _handler;

    public GetEventQueryHandlerTests()
    {
        _handler = new GetEventQueryHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_WithExistingEvent_ReturnsItsDto()
    {
        var @event = Event.Create("Fête nationale", ValidDate, Today);
        _repository.Events.Add(@event);

        var dto = await _handler.HandleAsync(new GetEventQuery(@event.Id), TestContext.Current.CancellationToken);

        dto.Should().Be(new EventDto(@event.Id.Value, "Fête nationale", ValidDate));
    }

    [Fact]
    public async Task HandleAsync_WithUnknownId_ReturnsNull()
    {
        _repository.Events.Add(Event.Create("Fête nationale", ValidDate, Today));

        var dto = await _handler.HandleAsync(new GetEventQuery(EventId.New()), TestContext.Current.CancellationToken);

        dto.Should().BeNull();
    }
}
