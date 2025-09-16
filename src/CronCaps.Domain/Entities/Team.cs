namespace CronCaps.Domain.Entities;

public class Team : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CreatedById { get; private set; }

    // Navigation properties
    public User CreatedBy { get; private set; } = null!;
    public ICollection<User> Members { get; private set; } = new List<User>();
    public ICollection<Job> Jobs { get; private set; } = new List<Job>();

    private Team() { } // For EF Core

    private Team(string name, Guid createdById)
    {
        Name = name;
        CreatedById = createdById;
        IsActive = true;
    }

    public static Team Create(string name, Guid createdById, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be null or empty", nameof(name));

        var team = new Team(name, createdById)
        {
            Description = description?.Trim()
        };

        return team;
    }

    public void UpdateDetails(string? name = null, string? description = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }

        Description = description?.Trim();
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
        UpdateTimestamp();
    }

    public void AddMember(User user)
    {
        if (Members.Contains(user)) return;

        Members.Add(user);
        UpdateTimestamp();
    }

    public void RemoveMember(User user)
    {
        if (!Members.Contains(user)) return;

        Members.Remove(user);
        UpdateTimestamp();
    }

    public bool HasMember(Guid userId)
    {
        return Members.Any(m => m.Id == userId);
    }

    public bool CanUserAccess(User user)
    {
        return HasMember(user.Id) || user.Id == CreatedById || user.HasPermission(Enums.UserRole.Admin);
    }

    public int GetActiveJobsCount()
    {
        return Jobs.Count(j => j.Status == Enums.JobStatus.Active);
    }

    public int GetTotalJobsCount()
    {
        return Jobs.Count(j => j.Status != Enums.JobStatus.Deleted);
    }

    public int GetMembersCount()
    {
        return Members.Count(m => m.IsActive);
    }
}