using AppointmentBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Api.Controllers;

[ApiController]
[Route("api/professionals/{professionalId:guid}/availability")]
public sealed class AvailabilityController(BookingDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AvailabilityResponse>> Get(Guid professionalId, [FromQuery] Guid serviceId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken)
    {
        if (to < from) return BadRequest("The 'to' date must not be before the 'from' date.");
        if (to.DayNumber - from.DayNumber > 31) return BadRequest("Availability searches are limited to 31 days.");
        var professional = await db.Professionals.AsNoTracking().SingleOrDefaultAsync(p => p.Id == professionalId && p.IsActive, cancellationToken);
        var service = await db.Services.AsNoTracking().SingleOrDefaultAsync(s => s.Id == serviceId && s.ProfessionalId == professionalId && s.IsActive, cancellationToken);
        if (professional is null || service is null) return NotFound();
        var zone = FindTimeZone(professional.TimeZoneId);
        var hours = await db.WorkingHours.AsNoTracking().Where(h => h.ProfessionalId == professionalId).ToListAsync(cancellationToken);
        var timeOff = await db.TimeOff.AsNoTracking().Where(t => t.ProfessionalId == professionalId).ToListAsync(cancellationToken);
        var slots = new List<AvailabilitySlot>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            foreach (var schedule in hours.Where(h => h.DayOfWeek == date.DayOfWeek))
            {
                for (var localStart = schedule.StartsAt; localStart.AddMinutes(service.DurationMinutes) <= schedule.EndsAt; localStart = localStart.AddMinutes(service.DurationMinutes))
                {
                    var localDateTime = date.ToDateTime(localStart);
                    if (zone.IsInvalidTime(localDateTime)) continue;
                    var startUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDateTime, zone));
                    var endUtc = startUtc.AddMinutes(service.DurationMinutes);
                    if (timeOff.Any(block => startUtc < block.EndsAtUtc && endUtc > block.StartsAtUtc)) continue;
                    slots.Add(new AvailabilitySlot(startUtc, endUtc, localDateTime));
                }
            }
        }
        return Ok(new AvailabilityResponse(professional.Id, professional.TimeZoneId, service.Id, service.Name, slots));
    }

    private static TimeZoneInfo FindTimeZone(string id)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
    }
}

public sealed record AvailabilityResponse(Guid ProfessionalId, string TimeZoneId, Guid ServiceId, string ServiceName, IReadOnlyList<AvailabilitySlot> Slots);
public sealed record AvailabilitySlot(DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc, DateTime LocalStart);
