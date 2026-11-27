using AppointmentBooking.Domain;
using AppointmentBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AppointmentBooking.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public sealed class AppointmentsController(BookingDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> Book(BookAppointmentRequest request, CancellationToken cancellationToken)
    {
        var customerId = GetUserId();
        var service = await db.Services.AsNoTracking().SingleOrDefaultAsync(s => s.Id == request.ServiceId && s.IsActive, cancellationToken);
        if (service is null)
        {
            return NotFound("Service was not found.");
        }
        var professional = await db.Professionals.AsNoTracking().SingleOrDefaultAsync(p => p.Id == service.ProfessionalId && p.IsActive, cancellationToken);
        if (professional is null)
        {
            return NotFound("Professional was not found.");
        }
        var starts = request.StartsAtUtc.ToUniversalTime();
        var ends = starts.AddMinutes(service.DurationMinutes);
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var overlap = await db.Appointments.AnyAsync(a => a.ProfessionalId == professional.Id && a.Status != AppointmentStatus.Cancelled && starts < a.EndsAtUtc && ends > a.StartsAtUtc, cancellationToken);
        if (overlap)
        {
            return Conflict("This appointment slot is no longer available.");
        }
        var appointment = new Appointment(Guid.NewGuid(), professional.Id, customerId, service.Id, starts, ends);
        db.Appointments.Add(appointment);
        db.AppointmentReminders.Add(new AppointmentReminder(Guid.NewGuid(), appointment.Id, starts.AddHours(-24)));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure })
        {
            return Conflict("The slot was reserved by another request.");
        }
        return Created($"/api/appointments/{appointment.Id}", AppointmentResponse.From(appointment));
    }

    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<AppointmentResponse>>> Mine(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var values = await db.Appointments.AsNoTracking().Where(a => a.CustomerId == userId).OrderByDescending(a => a.StartsAtUtc).Select(a => AppointmentResponse.From(a)).ToListAsync(cancellationToken);
        return Ok(values);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments.SingleOrDefaultAsync(a => a.Id == id && a.CustomerId == GetUserId(), cancellationToken);
        if (appointment is null)
        {
            return NotFound();
        }

        appointment.Cancel();
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var subject = User.FindFirst("sub")?.Value;

        if (subject is null)
        {
            throw new InvalidOperationException("Authenticated subject is missing.");
        }

        return Guid.Parse(subject);
    }
}

public sealed record BookAppointmentRequest(Guid ServiceId, DateTimeOffset StartsAtUtc);
public sealed record AppointmentResponse(Guid Id, Guid ProfessionalId, Guid CustomerId, Guid ServiceId, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc, AppointmentStatus Status)
{
    public static AppointmentResponse From(Appointment appointment)
    {
        return new AppointmentResponse(
            appointment.Id,
            appointment.ProfessionalId,
            appointment.CustomerId,
            appointment.ServiceId,
            appointment.StartsAtUtc,
            appointment.EndsAtUtc,
            appointment.Status);
    }
}
