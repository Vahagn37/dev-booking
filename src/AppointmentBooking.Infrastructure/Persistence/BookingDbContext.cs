using AppointmentBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Infrastructure.Persistence;

public sealed class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<ProfessionalProfile> Professionals => Set<ProfessionalProfile>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<WorkingHours> WorkingHours => Set<WorkingHours>();
    public DbSet<TimeOff> TimeOff => Set<TimeOff>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProfessionalProfile>(entity =>
        {
            entity.ToTable("professionals");
            entity.HasKey(profile => profile.Id);
            entity.Property(profile => profile.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(profile => profile.TimeZoneId).HasMaxLength(100).IsRequired();
            entity.HasIndex(profile => profile.DisplayName);
            entity.HasMany(profile => profile.Services).WithOne().HasForeignKey(service => service.ProfessionalId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("services");
            entity.HasKey(service => service.Id);
            entity.Property(service => service.Name).HasMaxLength(200).IsRequired();
            entity.Property(service => service.Price).HasPrecision(18, 2);
            entity.HasIndex(service => new { service.ProfessionalId, service.Name }).IsUnique();
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.Property(user => user.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(user => user.PasswordHash).IsRequired();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<WorkingHours>(entity =>
        {
            entity.ToTable("working_hours");
            entity.HasKey(hours => hours.Id);
            entity.Property(hours => hours.DayOfWeek).HasConversion<string>().HasMaxLength(12);
            entity.Property(hours => hours.StartsAt).HasColumnType("time");
            entity.Property(hours => hours.EndsAt).HasColumnType("time");
            entity.HasIndex(hours => new { hours.ProfessionalId, hours.DayOfWeek, hours.StartsAt }).IsUnique();
        });

        modelBuilder.Entity<TimeOff>(entity =>
        {
            entity.ToTable("time_off");
            entity.HasKey(timeOff => timeOff.Id);
            entity.Property(timeOff => timeOff.StartsAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(timeOff => timeOff.EndsAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(timeOff => timeOff.Reason).HasMaxLength(500);
            entity.HasIndex(timeOff => new { timeOff.ProfessionalId, timeOff.StartsAtUtc });
        });
    }
}
