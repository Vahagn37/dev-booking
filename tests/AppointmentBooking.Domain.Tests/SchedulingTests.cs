using AppointmentBooking.Domain;

namespace AppointmentBooking.Domain.Tests;

public sealed class SchedulingTests
{
    [Fact]
    public void WorkingHours_RejectsReversedInterval() => Assert.Throws<ArgumentException>(() => new WorkingHours(Guid.NewGuid(), Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(17), new TimeOnly(9)));

    [Fact]
    public void TimeOff_NormalizesInstantsToUtc()
    {
        var value = new TimeOff(Guid.NewGuid(), Guid.NewGuid(), new DateTimeOffset(2026, 11, 27, 10, 0, 0, TimeSpan.FromHours(4)), new DateTimeOffset(2026, 11, 27, 11, 0, 0, TimeSpan.FromHours(4)), null);
        Assert.Equal(TimeSpan.Zero, value.StartsAtUtc.Offset);
    }
}
