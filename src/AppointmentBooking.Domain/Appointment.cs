namespace AppointmentBooking.Domain;

public enum AppointmentStatus { Scheduled, Cancelled, Completed }

public sealed class Appointment
{
    private Appointment() { }

    public Appointment(Guid id, Guid professionalId, Guid customerId, Guid serviceId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc)
    {
        if (startsAtUtc >= endsAtUtc) throw new ArgumentException("Appointment start must be before its end.");
        Id = id; ProfessionalId = professionalId; CustomerId = customerId; ServiceId = serviceId;
        StartsAtUtc = startsAtUtc.ToUniversalTime(); EndsAtUtc = endsAtUtc.ToUniversalTime(); Status = AppointmentStatus.Scheduled;
    }

    public Guid Id { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public void Cancel()
    {
        if (Status == AppointmentStatus.Scheduled)
        {
            Status = AppointmentStatus.Cancelled;
        }
    }

    public void Reschedule(DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc)
    {
        if (Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException(
                "Only scheduled appointments can be rescheduled.");
        }

        StartsAtUtc = startsAtUtc.ToUniversalTime();
        EndsAtUtc = endsAtUtc.ToUniversalTime();
    }
}
