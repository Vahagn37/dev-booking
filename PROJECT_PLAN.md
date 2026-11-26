# Appointment Booking System — Project Plan

## Goal

Build a production-minded ASP.NET Core API where customers can discover a professional's availability and book, reschedule, or cancel appointments without double-booking a time slot. The project should emphasize business rules, time zones, transactions, concurrency control, and reliable reminders.

## Scope

### MVP

- Professional profiles and services (for example, 30-minute consultation or 60-minute haircut).
- Weekly working hours in each professional's IANA time zone.
- Customer registration and JWT authentication.
- Public available-slot search over a date range.
- Authenticated appointment booking, rescheduling, and cancellation.
- Prevention of overlapping appointments for one professional.
- Admin API for schedules, services, holidays, and appointment history.
- Validation, problem-details error responses, OpenAPI, database migrations, and automated tests.

### Later extensions

- Recurring schedules, holidays, blackout dates, and buffer time between appointments.
- Cancellation window and no-show policies.
- Reminder emails through background jobs.
- Calendar exports and webhooks.
- Multi-tenant organizations, staff roles, and payment/deposit handling.

## Recommended Stack

- .NET 10 / ASP.NET Core Web API with controllers.
- PostgreSQL + EF Core + Npgsql.
- ASP.NET Core Identity or a small custom user model with JWT bearer authentication.
- FluentValidation, xUnit, FluentAssertions, and Testcontainers for integration tests.
- Hangfire or Quartz for reminder delivery (add after the booking flow is correct).
- Docker Compose for the API, PostgreSQL, and optional local email viewer.

## Core Model

| Aggregate | Important fields | Rules |
| --- | --- | --- |
| User | Id, Email, DisplayName, Role | A user can be Customer, Professional, or Admin. |
| ProfessionalProfile | UserId, TimeZoneId, IsActive | `TimeZoneId` must be an IANA identifier. |
| Service | Id, ProfessionalId, Name, DurationMinutes, Price, IsActive | Duration is positive; an inactive service cannot be newly booked. |
| WorkingHours | ProfessionalId, DayOfWeek, StartLocalTime, EndLocalTime | Intervals must not overlap for the same day. |
| TimeOff | ProfessionalId, StartsAtUtc, EndsAtUtc, Reason | Blocks booking during the interval. |
| Appointment | Id, ProfessionalId, CustomerId, ServiceId, StartsAtUtc, EndsAtUtc, Status, Version | Times are stored as UTC; state is Scheduled, Cancelled, or Completed. |

Use `DateTimeOffset`/UTC for persisted appointment instants. Convert a professional's local working hours into UTC only for the searched date range. Never store an appointment as an unspecified local `DateTime`.

## API Surface

- `POST /api/auth/register`, `POST /api/auth/login`
- `GET /api/professionals?serviceId=...`
- `GET /api/professionals/{id}/availability?from=...&to=...&serviceId=...`
- `POST /api/appointments` — customer books an available slot.
- `POST /api/appointments/{id}/reschedule`
- `POST /api/appointments/{id}/cancel`
- `GET /api/appointments/me` — role-aware customer or professional history.
- `POST|PUT|DELETE /api/admin/professionals/{id}/services`
- `POST|PUT|DELETE /api/admin/professionals/{id}/working-hours`
- `POST|DELETE /api/admin/professionals/{id}/time-off`

## The Critical Booking Design

Availability is a helpful read model, not a reservation. The `POST /appointments` command must re-check every rule inside a database transaction:

1. Load the active service and calculate `EndsAtUtc` from the requested start.
2. Confirm the request lies within the professional's working hours and outside time off.
3. Lock or serialize bookings for the professional/date and reject any overlap with a non-cancelled appointment.
4. Insert the appointment and commit.

For PostgreSQL, implement one robust database-level safeguard in addition to the application check: an exclusion constraint on a `tstzrange(StartsAtUtc, EndsAtUtc)` for each `ProfessionalId`, excluding cancelled rows. This makes simultaneous requests safe even if application code later changes. If EF migrations cannot express it cleanly, add the constraint with migration SQL and document the decision in an ADR.

Return `409 Conflict` for a slot that was just taken; the client should refresh availability and choose another time. Use an optimistic-concurrency version column on appointments for rescheduling/cancellation updates.

## Architecture

Start as a modular monolith:

```text
src/
  AppointmentBooking.Api/             HTTP, auth, middleware, OpenAPI
  AppointmentBooking.Application/     use cases, validation, interfaces
  AppointmentBooking.Domain/          entities, value objects, domain rules
  AppointmentBooking.Infrastructure/  EF Core, PostgreSQL, email, jobs
tests/
  AppointmentBooking.Domain.Tests/
  AppointmentBooking.IntegrationTests/
docs/
  adr/
```

Keep booking, scheduling, and notifications as clear modules, but do not split services. A single deployable application with explicit boundaries teaches the right trade-offs for this project.

## Milestones

### 1. Foundation — 1 week

- Create the solution and projects; add PostgreSQL, EF Core, Docker Compose, health checks, structured logging, and OpenAPI.
- Add a consistent Problem Details error format and request validation.
- Deliverable: `docker compose up` runs the API and `/health/ready` checks the database.

### 2. Identity and configuration — 1 week

- Implement registration/login and JWT bearer authentication.
- Seed an admin and protect admin routes with policies.
- Keep secrets in local user secrets or environment variables—not source control.
- Deliverable: integration tests prove unauthenticated and unauthorized access is rejected.

### 3. Schedule and catalog management — 1 week

- Implement professional profile, services, weekly hours, and time-off management.
- Validate overlapping working-hour definitions and invalid time zones.
- Deliverable: admin can configure a bookable professional completely.

### 4. Availability calculation — 1–2 weeks

- Generate candidate slots from working hours, service duration, and optional buffer.
- Exclude time off and existing appointments.
- Test daylight-saving transitions, including an ambiguous fall-back local time and a nonexistent spring-forward time.
- Deliverable: a customer receives correct slots for a range in the professional's local time, with UTC instants in the response.

### 5. Safe booking workflow — 1–2 weeks

- Implement booking, cancellation, and rescheduling with explicit state transitions.
- Add the PostgreSQL overlap constraint and transactional rule checks.
- Write a concurrency integration test that sends two simultaneous booking requests for the same slot; exactly one must succeed.
- Deliverable: no double booking under concurrent requests.

### 6. Reminders and operations — 1 week

- Add a background reminder job, idempotent delivery tracking, retry policy, and a fake email sender in development.
- Add logs with appointment IDs, traces, metrics, rate limiting, and readiness/liveness health endpoints.
- Deliverable: an upcoming appointment yields one reminder despite job retries.

### 7. Portfolio finish — 1 week

- Write the README, API examples, architecture diagram, ADRs, deployment instructions, and a short demo script.
- Deploy the API and monitor its health/logs.
- Deliverable: someone else can clone, configure, test, and run the project.

## Test Strategy

- Unit tests: interval overlap logic, slot calculation, state transitions, cancellation policy, and time-zone conversion.
- Integration tests: migrations, auth policies, EF queries, PostgreSQL exclusion constraint, and real transaction behavior using Testcontainers.
- API tests: validation errors, pagination/filtering, authorization boundaries, and Problem Details responses.
- Failure drills: duplicate booking submissions, two parallel reservations, database outage, cancelled appointment reminder, expired JWT, and a request for another professional's appointment.

## Definition of Done

The project is ready for portfolio review when all endpoints are documented, migrations apply from a clean database, Docker runs the complete local stack, concurrency tests prove no double booking, time-zone/DST cases are covered, and the README explains the technical trade-offs.

## First Build Slice

Create the solution, PostgreSQL Compose service, health checks, a `ProfessionalProfile` entity, a `Service` entity, and a migration. Seed one professional in `America/New_York` with a 30-minute consultation. Do not build the availability endpoint until this vertical slice is tested end to end.
