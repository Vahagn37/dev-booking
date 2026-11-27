using AppointmentBooking.Domain;
using AppointmentBooking.Infrastructure.Auth;
using AppointmentBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(BookingDbContext db, IPasswordHasher<UserAccount> hasher, JwtTokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!IsValidPassword(request.Password))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid password",
                Detail = "Password must be at least 8 characters and include a number and symbol."
            });
        }
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            return Conflict(new ProblemDetails { Title = "Email already registered" });
        }
        var user = new UserAccount(Guid.NewGuid(), email, request.DisplayName);
        user.SetPasswordHash(hasher.HashPassword(user, request.Password));
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new AuthResponse(tokens.CreateToken(user), user.Id, user.Email, user.DisplayName, user.Role.ToString()));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(candidate => candidate.Email == request.Email.Trim().ToLowerInvariant() && candidate.IsActive, cancellationToken);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new ProblemDetails { Title = "Invalid credentials" });
        return Ok(new AuthResponse(tokens.CreateToken(user), user.Id, user.Email, user.DisplayName, user.Role.ToString()));
    }

    private static bool IsValidPassword(string password)
    {
        return password.Length >= 8
            && password.Any(char.IsDigit)
            && password.Any(character => !char.IsLetterOrDigit(character));
    }
}

public sealed record RegisterRequest(string Email, string DisplayName, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string AccessToken, Guid UserId, string Email, string DisplayName, string Role);
