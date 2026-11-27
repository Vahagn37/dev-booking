using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AppointmentBooking.Infrastructure.Auth;

public static class AuthDependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher<AppointmentBooking.Domain.UserAccount>, PasswordHasher<AppointmentBooking.Domain.UserAccount>>();
        services.AddScoped<JwtTokenService>();
        return services;
    }
}
