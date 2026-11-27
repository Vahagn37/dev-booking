namespace AppointmentBooking.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "AppointmentBooking";
    public string Audience { get; init; } = "AppointmentBooking.Api";
    public string SigningKey { get; init; } = "development-only-change-this-signing-key-please";
    public int ExpirationMinutes { get; init; } = 60;
}
