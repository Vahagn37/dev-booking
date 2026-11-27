using AppointmentBooking.Infrastructure.Persistence;
using AppointmentBooking.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppointmentBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BookingDatabase")
            ?? throw new InvalidOperationException("Connection string 'BookingDatabase' is required.");
        services.AddDbContext<BookingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddAuthInfrastructure();
        return services;
    }
}
