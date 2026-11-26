using AppointmentBooking.Domain;

namespace AppointmentBooking.Domain.Tests;

public sealed class ServiceTests
{
    [Fact]
    public void Constructor_RejectsNonPositiveDuration() => Assert.Throws<ArgumentOutOfRangeException>(() => new Service(Guid.NewGuid(), Guid.NewGuid(), "Consultation", 0, 75m));

    [Fact]
    public void Constructor_RejectsNegativePrice() => Assert.Throws<ArgumentOutOfRangeException>(() => new Service(Guid.NewGuid(), Guid.NewGuid(), "Consultation", 30, -1m));
}
