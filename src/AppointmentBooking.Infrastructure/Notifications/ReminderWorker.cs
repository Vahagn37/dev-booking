using AppointmentBooking.Domain;
using AppointmentBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppointmentBooking.Infrastructure.Notifications;

public sealed class ReminderWorker(IServiceScopeFactory scopeFactory, ILogger<ReminderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try
            {
                await ProcessDueRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Reminder processing failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessDueRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var now = DateTimeOffset.UtcNow;
        var reminders = await db.AppointmentReminders
            .Include(reminder => reminder.Appointment)
            .ThenInclude(appointment => appointment.Customer)
            .Where(reminder => reminder.SentAtUtc == null && reminder.ScheduledForUtc <= now && reminder.Attempts < 3 && reminder.Appointment.Status == AppointmentStatus.Scheduled)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var reminder in reminders)
        {
            try
            {
                await sender.SendAppointmentReminderAsync(reminder.Appointment.Customer.Email, reminder.Appointment.StartsAtUtc, cancellationToken);
                reminder.MarkSent(now);
            }
            catch (Exception exception)
            {
                reminder.MarkFailed(exception.Message);
                logger.LogWarning(exception, "Reminder {ReminderId} failed on attempt {Attempt}.", reminder.Id, reminder.Attempts);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
