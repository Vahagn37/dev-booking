using AppointmentBooking.Domain;
using AppointmentBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Api.Controllers;

[ApiController]
[Route("api/professionals")]
public sealed class ProfessionalsController(BookingDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProfessionalResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var professionals = await dbContext.Professionals.AsNoTracking().Include(professional => professional.Services)
            .OrderBy(professional => professional.DisplayName).ToListAsync(cancellationToken);
        return Ok(professionals.Select(ProfessionalResponse.From).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ProfessionalResponse>> Create(CreateProfessionalRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetTimeZone(request.TimeZoneId, out var problem))
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]> { [nameof(request.TimeZoneId)] = [problem!] }));

        var professional = new ProfessionalProfile(Guid.NewGuid(), request.DisplayName, request.TimeZoneId);
        dbContext.Professionals.Add(professional);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = professional.Id }, ProfessionalResponse.From(professional));
    }

    [HttpPost("{professionalId:guid}/services")]
    public async Task<ActionResult<ServiceResponse>> AddService(Guid professionalId, CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var professional = await dbContext.Professionals.Include(profile => profile.Services)
            .SingleOrDefaultAsync(profile => profile.Id == professionalId, cancellationToken);
        if (professional is null)
        {
            return NotFound();
        }

        var service = professional.AddService(Guid.NewGuid(), request.Name, request.DurationMinutes, request.Price);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Created($"/api/services/{service.Id}", ServiceResponse.From(service));
    }

    private static bool TryGetTimeZone(string? timeZoneId, out string? problem)
    {
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId ?? string.Empty);
            problem = null;
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            problem = "TimeZoneId must be a valid IANA identifier, such as America/New_York.";
            return false;
        }
    }
}

public sealed record CreateProfessionalRequest(string DisplayName, string TimeZoneId);
public sealed record CreateServiceRequest(string Name, int DurationMinutes, decimal Price);
public sealed record ProfessionalResponse(Guid Id, string DisplayName, string TimeZoneId, bool IsActive, IReadOnlyList<ServiceResponse> Services)
{
    public static ProfessionalResponse From(ProfessionalProfile profile)
    {
        return new ProfessionalResponse(
            profile.Id,
            profile.DisplayName,
            profile.TimeZoneId,
            profile.IsActive,
            profile.Services.Select(ServiceResponse.From).ToList());
    }
}
public sealed record ServiceResponse(Guid Id, string Name, int DurationMinutes, decimal Price, bool IsActive)
{
    public static ServiceResponse From(Service service)
    {
        return new ServiceResponse(service.Id, service.Name, service.DurationMinutes, service.Price, service.IsActive);
    }
}
