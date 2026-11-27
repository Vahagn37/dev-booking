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
}
