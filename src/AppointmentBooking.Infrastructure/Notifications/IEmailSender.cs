namespace AppointmentBooking.Infrastructure.Notifications;

public interface IEmailSender
{
    Task SendAppointmentReminderAsync(string recipient, DateTimeOffset startsAtUtc, CancellationToken cancellationToken);
}
