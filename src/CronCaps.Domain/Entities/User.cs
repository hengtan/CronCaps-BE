using CronCaps.Domain.Enums;
using CronCaps.Domain.ValueObjects;

namespace CronCaps.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    // Navigation properties
    public ICollection<Job> Jobs { get; private set; } = new List<Job>();
    public ICollection<Team> Teams { get; private set; } = new List<Team>();

    private User() { } // For EF Core

    private User(string username, Email email, string passwordHash, UserRole role)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    public static User Create(
        string username,
        string email,
        string passwordHash,
        UserRole role = UserRole.User,
        string? firstName = null,
        string? lastName = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));

        var user = new User(username, Email.Create(email), passwordHash, role)
        {
            FirstName = firstName?.Trim(),
            LastName = lastName?.Trim()
        };

        return user;
    }

    public void UpdateProfile(string? firstName, string? lastName)
    {
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        UpdateTimestamp();
    }

    public void UpdateEmail(string newEmail)
    {
        Email = Email.Create(newEmail);
        UpdateTimestamp();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        UpdateTimestamp();
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
        UpdateTimestamp();
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        UpdateTimestamp();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void SetRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
        UpdateTimestamp();
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        UpdateTimestamp();
    }

    public bool IsRefreshTokenValid()
    {
        return !string.IsNullOrEmpty(RefreshToken) &&
               RefreshTokenExpiryTime.HasValue &&
               RefreshTokenExpiryTime > DateTime.UtcNow;
    }

    public string GetFullName()
    {
        var parts = new[] { FirstName, LastName }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        return parts.Length > 0 ? string.Join(" ", parts) : Username;
    }

    public bool HasPermission(UserRole requiredRole)
    {
        return Role >= requiredRole;
    }

    public bool CanManageUser(User targetUser)
    {
        if (Id == targetUser.Id) return true; // Can manage self

        return Role switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => targetUser.Role < UserRole.Admin,
            UserRole.Manager => targetUser.Role < UserRole.Manager,
            _ => false
        };
    }
}