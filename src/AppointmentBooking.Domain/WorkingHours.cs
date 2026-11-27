namespace AppointmentBooking.Domain;

public sealed class WorkingHours
{
    private WorkingHours() { }

    public WorkingHours(Guid id, Guid professionalId, DayOfWeek dayOfWeek, TimeOnly startsAt, TimeOnly endsAt)
    {
        if (startsAt >= endsAt) throw new ArgumentException("Working-hours start must be before its end.");
        Id = id;
        ProfessionalId = professionalId;
        DayOfWeek = dayOfWeek;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public Guid Id { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartsAt { get; private set; }
    public TimeOnly EndsAt { get; private set; }
}
