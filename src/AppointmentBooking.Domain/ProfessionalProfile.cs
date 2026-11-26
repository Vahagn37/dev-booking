namespace AppointmentBooking.Domain;

public sealed class ProfessionalProfile
{
    private ProfessionalProfile()
    {
    }

    public ProfessionalProfile(Guid id, string displayName, string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("A professional must have a display name.", nameof(displayName));
        }

        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException("A professional must have an IANA time zone.", nameof(timeZoneId));
        }

        Id = id;
        DisplayName = displayName.Trim();
        TimeZoneId = timeZoneId.Trim();
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string DisplayName { get; private set; } = null!;
    public string TimeZoneId { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public ICollection<Service> Services { get; } = new List<Service>();

    public Service AddService(Guid id, string name, int durationMinutes, decimal price)
    {
        var service = new Service(id, Id, name, durationMinutes, price);
        Services.Add(service);
        return service;
    }
}
