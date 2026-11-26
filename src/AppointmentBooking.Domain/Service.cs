namespace AppointmentBooking.Domain;

public sealed class Service
{
    private Service()
    {
    }

    public Service(Guid id, Guid professionalId, string name, int durationMinutes, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A service must have a name.", nameof(name));
        }

        if (durationMinutes <= 0 || durationMinutes > 480)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMinutes), "Duration must be between 1 and 480 minutes.");
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        Id = id;
        ProfessionalId = professionalId;
        Name = name.Trim();
        DurationMinutes = durationMinutes;
        Price = price;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public string Name { get; private set; } = null!;
    public int DurationMinutes { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
}
