namespace AppointmentBooking.Domain;

public enum UserRole
{
    Customer,
    Professional,
    Admin
}

public sealed class UserAccount
{
    private UserAccount() { }

    public UserAccount(Guid id, string email, string displayName, UserRole role = UserRole.Customer)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));
        Id = id;
        Email = email.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();
        Role = role;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;
}
