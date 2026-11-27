using AppointmentBooking.Domain;

namespace AppointmentBooking.Domain.Tests;

public sealed class ReminderTests
{
    [Fact]
    public void Cancel_PreventsPendingDelivery()
    {
        var reminder = new AppointmentReminder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        reminder.Cancel();

        Assert.NotNull(reminder.SentAtUtc);
        Assert.Equal("Appointment cancelled.", reminder.LastError);
    }

    [Fact]
    public void Reschedule_ClearsPreviousDeliveryState()
    {
        var reminder = new AppointmentReminder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
        reminder.MarkSent(DateTimeOffset.UtcNow);

        var newTime = DateTimeOffset.UtcNow.AddDays(1);
        reminder.Reschedule(newTime);

        Assert.Null(reminder.SentAtUtc);
        Assert.Null(reminder.LastError);
        Assert.Equal(newTime.ToUniversalTime(), reminder.ScheduledForUtc);
    }
}
