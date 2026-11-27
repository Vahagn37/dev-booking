using Xunit;

namespace AppointmentBooking.Domain.Tests;

public sealed class AvailabilityTests
{
    [Fact]
    public void SpringForwardDay_ContainsInvalidLocalTimes()
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        Assert.True(zone.IsInvalidTime(new DateTime(2026, 3, 8, 2, 30, 0)));
    }
}
