using EventsManager.Application;

namespace EventsManager.UnitTests.Application;

public class TimeProviderExtensionsTests
{
    [Fact]
    public void TodayLocal_Returns_LocalDatePart()
    {
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(2026, 7, 13, 23, 30, 0, TimeSpan.Zero));

        timeProvider.TodayLocal().Should().Be(new DateOnly(2026, 7, 13));
    }
}
