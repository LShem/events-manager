using EventsManager.Domain.Orders;

namespace EventsManager.UnitTests.Domain.Orders;

/// <summary>
/// OR-C7 : identifiant OrderId typé, généré à la création, même famille qu'EventId —
/// mêmes garanties que <c>EventIdTests</c> (UUIDv7, unicité, rejet du Guid vide).
/// </summary>
public class OrderIdTests
{
    [Fact]
    public void New_Returns_NonEmptyValue()
    {
        var id = OrderId.New();

        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void New_Returns_Version7Guid()
    {
        var id = OrderId.New();

        id.Value.Version.Should().Be(7);
    }

    [Fact]
    public void New_Returns_UniqueValues()
    {
        var ids = Enumerable.Range(0, 1000).Select(_ => OrderId.New()).ToArray();

        ids.Distinct().Should().HaveCount(1000);
    }

    [Fact]
    public void From_Rejects_EmptyGuid()
    {
        Action act = () => OrderId.From(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_Preserves_Value()
    {
        var guid = Guid.CreateVersion7();

        var id = OrderId.From(guid);

        id.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_Returns_GuidString()
    {
        var guid = Guid.CreateVersion7();

        OrderId.From(guid).ToString().Should().Be(guid.ToString());
    }
}
