using AppointmentBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Infrastructure.Persistence;

public static class BookingDbSeeder
{
    public static readonly Guid DemoProfessionalId = Guid.Parse("649C3A9E-8D09-45CE-BAB1-4B20C5A486B2");

    public static async Task SeedAsync(BookingDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Professionals.AnyAsync(cancellationToken)) return;

        var professional = new ProfessionalProfile(DemoProfessionalId, "Morgan Chen", "America/New_York");
        professional.AddService(Guid.Parse("CC4B502F-302F-471D-81A5-2D7A2088B616"), "30-minute consultation", 30, 75m);
        dbContext.Professionals.Add(professional);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
