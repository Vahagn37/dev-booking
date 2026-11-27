using AppointmentBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace AppointmentBooking.Infrastructure.Persistence;

public static class BookingDbSeeder
{
    public static readonly Guid DemoProfessionalId = Guid.Parse("649C3A9E-8D09-45CE-BAB1-4B20C5A486B2");

    public static async Task SeedAsync(BookingDbContext dbContext, IPasswordHasher<UserAccount> passwordHasher, CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Users.AnyAsync(user => user.Email == "admin@example.com", cancellationToken))
        {
            var admin = new UserAccount(Guid.Parse("DCE7D7E4-74C9-4F98-9B83-3D8C5A8C1D4E"), "admin@example.com", "System Admin", UserRole.Admin);
            admin.SetPasswordHash(passwordHasher.HashPassword(admin, "Admin123!ChangeMe"));
            dbContext.Users.Add(admin);
        }

        if (await dbContext.Professionals.AnyAsync(cancellationToken))
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var professional = new ProfessionalProfile(DemoProfessionalId, "Morgan Chen", "America/New_York");
        professional.AddService(Guid.Parse("CC4B502F-302F-471D-81A5-2D7A2088B616"), "30-minute consultation", 30, 75m);
        dbContext.Professionals.Add(professional);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
