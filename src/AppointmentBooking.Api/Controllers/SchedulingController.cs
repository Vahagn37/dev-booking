using AppointmentBooking.Domain;
using AppointmentBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Api.Controllers;

[ApiController]
[Route("api/admin/professionals/{professionalId:guid}/schedule")]
[Authorize(Policy = "AdminOnly")]
public sealed class SchedulingController(BookingDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ScheduleResponse>> Get(Guid professionalId, CancellationToken cancellationToken)
    {
        if (!await db.Professionals.AnyAsync(p => p.Id == professionalId, cancellationToken))
        {
            return NotFound();
        }
        var hours = await db.WorkingHours.AsNoTracking().Where(h => h.ProfessionalId == professionalId).OrderBy(h => h.DayOfWeek).ThenBy(h => h.StartsAt).ToListAsync(cancellationToken);
        var timeOff = await db.TimeOff.AsNoTracking().Where(t => t.ProfessionalId == professionalId && t.EndsAtUtc >= DateTimeOffset.UtcNow).OrderBy(t => t.StartsAtUtc).ToListAsync(cancellationToken);
        return Ok(new ScheduleResponse(hours.Select(WorkingHoursResponse.From), timeOff.Select(TimeOffResponse.From)));
    }

    [HttpPost("working-hours")]
    public async Task<ActionResult<WorkingHoursResponse>> AddHours(Guid professionalId, AddWorkingHoursRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Professionals.AnyAsync(p => p.Id == professionalId, cancellationToken))
        {
            return NotFound();
        }

        if (!Enum.TryParse<DayOfWeek>(request.DayOfWeek, true, out var day))
        {
            return BadRequest("DayOfWeek must be a valid day name.");
        }
        WorkingHours hours;
        try
        {
            hours = new WorkingHours(Guid.NewGuid(), professionalId, day, request.StartsAt, request.EndsAt);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        var overlaps = await db.WorkingHours.AnyAsync(h => h.ProfessionalId == professionalId && h.DayOfWeek == day && request.StartsAt < h.EndsAt && request.EndsAt > h.StartsAt, cancellationToken);
        if (overlaps)
        {
            return Conflict("Working hours overlap an existing interval.");
        }
        db.WorkingHours.Add(hours);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/admin/professionals/{professionalId}/schedule", WorkingHoursResponse.From(hours));
    }

    [HttpPost("time-off")]
    public async Task<ActionResult<TimeOffResponse>> AddTimeOff(Guid professionalId, AddTimeOffRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Professionals.AnyAsync(p => p.Id == professionalId, cancellationToken))
        {
            return NotFound();
        }
        TimeOff timeOff;
        try
        {
            timeOff = new TimeOff(Guid.NewGuid(), professionalId, request.StartsAtUtc, request.EndsAtUtc, request.Reason);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        db.TimeOff.Add(timeOff);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/admin/professionals/{professionalId}/schedule", TimeOffResponse.From(timeOff));
    }
}

public sealed record AddWorkingHoursRequest(string DayOfWeek, TimeOnly StartsAt, TimeOnly EndsAt);
public sealed record AddTimeOffRequest(DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc, string? Reason);
public sealed record ScheduleResponse(IEnumerable<WorkingHoursResponse> WorkingHours, IEnumerable<TimeOffResponse> TimeOff);
public sealed record WorkingHoursResponse(Guid Id, string DayOfWeek, TimeOnly StartsAt, TimeOnly EndsAt)
{
    public static WorkingHoursResponse From(WorkingHours value)
    {
        return new WorkingHoursResponse(value.Id, value.DayOfWeek.ToString(), value.StartsAt, value.EndsAt);
    }
}
public sealed record TimeOffResponse(Guid Id, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc, string? Reason)
{
    public static TimeOffResponse From(TimeOff value)
    {
        return new TimeOffResponse(value.Id, value.StartsAtUtc, value.EndsAtUtc, value.Reason);
    }
}
