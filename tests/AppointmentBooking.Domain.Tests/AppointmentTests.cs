using AppointmentBooking.Domain;

namespace AppointmentBooking.Domain.Tests;

public sealed class AppointmentTests
{
    [Fact]
    public void Cancel_ChangesStatus()
    {
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(30));
        appointment.Cancel();
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public void Reschedule_ChangesAppointmentWindow()
    {
        var appointment = new Appointment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30));
        var startsAtUtc = DateTimeOffset.UtcNow.AddDays(1);

        appointment.Reschedule(startsAtUtc, startsAtUtc.AddMinutes(30));

        Assert.Equal(startsAtUtc.ToUniversalTime(), appointment.StartsAtUtc);
        Assert.Equal(startsAtUtc.AddMinutes(30).ToUniversalTime(), appointment.EndsAtUtc);
    }
}
