using Microsoft.Extensions.Logging;

namespace AppointmentBooking.Infrastructure.Notifications;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAppointmentReminderAsync(string recipient, DateTimeOffset startsAtUtc, CancellationToken cancellationToken)
    {
        logger.LogInformation("Appointment reminder sent to {Recipient} for {StartsAtUtc}.", recipient, startsAtUtc);
        return Task.CompletedTask;
    }
}
