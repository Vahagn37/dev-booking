using AppointmentBooking.Infrastructure;
using AppointmentBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks().AddDbContextCheck<BookingDbContext>("database");

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapControllers();

if (app.Configuration.GetValue("Database:ApplyMigrations", false))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    // EnsureCreated keeps the first vertical slice runnable while the checked-in
    // migration is introduced in the persistence milestone.
    await dbContext.Database.EnsureCreatedAsync();
    await BookingDbSeeder.SeedAsync(dbContext);
}

app.Run();

public partial class Program;
