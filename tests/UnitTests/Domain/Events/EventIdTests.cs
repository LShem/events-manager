using EventsManager.Domain.Events;

namespace EventsManager.UnitTests.Domain.Events;

public class EventIdTests
{
    [Fact]
    public void New_Returns_NonEmptyValue()
    {
        var id = EventId.New();

        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void New_Returns_Version7Guid()
    {
        var id = EventId.New();

        id.Value.Version.Should().Be(7);
    }

    [Fact]
    public void New_Returns_UniqueValues()
    {
        var ids = Enumerable.Range(0, 1000).Select(_ => EventId.New()).ToArray();

        ids.Distinct().Should().HaveCount(1000);
    }

    [Fact]
    public void From_Rejects_EmptyGuid()
    {
        Action act = () => EventId.From(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_Preserves_Value()
    {
        var guid = Guid.CreateVersion7();

        var id = EventId.From(guid);

        id.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_Returns_GuidString()
    {
        var guid = Guid.CreateVersion7();

        EventId.From(guid).ToString().Should().Be(guid.ToString());
    }
}
