# Appointment Booking System

A production-minded ASP.NET Core API for professional appointment booking. The core engineering challenge is making concurrent bookings safe while preserving correct local-time behavior.

## Current milestone

Foundation is in place: modular solution layout, PostgreSQL, EF Core persistence, health endpoints, a seeded professional/service, and professional catalog endpoints. Booking, availability, authentication, and reminders come next.

## Run locally

Start PostgreSQL:

```bash
docker compose up -d postgres
```

Run the API with migrations and seed data:

```bash
Database__ApplyMigrations=true dotnet run --project src/AppointmentBooking.Api
```

Then verify:

```text
GET http://localhost:5180/health/live
GET http://localhost:5180/health/ready
GET http://localhost:5180/api/professionals
```

## Architecture

```text
src/
  AppointmentBooking.Api/             HTTP and application host
  AppointmentBooking.Application/     future use cases and validation
  AppointmentBooking.Domain/          business entities and invariants
  AppointmentBooking.Infrastructure/  EF Core and PostgreSQL persistence
tests/
  AppointmentBooking.Domain.Tests/
  AppointmentBooking.IntegrationTests/
```

See [PROJECT_PLAN.md](PROJECT_PLAN.md) for milestones and the concurrency design.
