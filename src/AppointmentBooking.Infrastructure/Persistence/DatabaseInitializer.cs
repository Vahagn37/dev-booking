using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(BookingDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (db.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            return;
        }

        await db.Database.ExecuteSqlRawAsync("""
            CREATE EXTENSION IF NOT EXISTS btree_gist;
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'no_overlapping_appointments'
                ) THEN
                    ALTER TABLE appointments
                    ADD CONSTRAINT no_overlapping_appointments
                    EXCLUDE USING gist (
                        "ProfessionalId" WITH =,
                        tstzrange("StartsAtUtc", "EndsAtUtc", '[)') WITH &&
                    ) WHERE ("Status" <> 'Cancelled');
                END IF;
            END $$;
            """, cancellationToken);
    }
}
