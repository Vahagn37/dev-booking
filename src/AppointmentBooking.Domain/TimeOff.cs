namespace AppointmentBooking.Domain;

public sealed class TimeOff
{
    private TimeOff() { }

    public TimeOff(Guid id, Guid professionalId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, string? reason)
    {
        if (startsAtUtc >= endsAtUtc) throw new ArgumentException("Time-off start must be before its end.");
        Id = id;
        ProfessionalId = professionalId;
        StartsAtUtc = startsAtUtc.ToUniversalTime();
        EndsAtUtc = endsAtUtc.ToUniversalTime();
        Reason = reason?.Trim();
    }

    public Guid Id { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }
    public string? Reason { get; private set; }
}
