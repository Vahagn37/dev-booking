namespace AppointmentBooking.Domain;

public sealed class AppointmentReminder
{
    private AppointmentReminder() { }

    public AppointmentReminder(Guid id, Guid appointmentId, DateTimeOffset scheduledForUtc)
    {
        Id = id;
        AppointmentId = appointmentId;
        ScheduledForUtc = scheduledForUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid AppointmentId { get; private set; }
    public Appointment Appointment { get; private set; } = null!;
    public DateTimeOffset ScheduledForUtc { get; private set; }
    public DateTimeOffset? SentAtUtc { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    public void MarkSent(DateTimeOffset sentAtUtc)
    {
        SentAtUtc = sentAtUtc.ToUniversalTime();
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        Attempts++;
        LastError = error;
    }
}
