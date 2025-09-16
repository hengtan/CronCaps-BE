namespace CronCaps.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Color { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CreatedById { get; private set; }

    // Navigation properties
    public User CreatedBy { get; private set; } = null!;
    public ICollection<Job> Jobs { get; private set; } = new List<Job>();

    private Category() { } // For EF Core

    private Category(string name, string color, Guid createdById)
    {
        Name = name;
        Color = color;
        CreatedById = createdById;
        IsActive = true;
    }

    public static Category Create(
        string name,
        Guid createdById,
        string? description = null,
        string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be null or empty", nameof(name));

        var normalizedColor = NormalizeColor(color ?? "#6366f1"); // Default indigo color

        var category = new Category(name, normalizedColor, createdById)
        {
            Description = description?.Trim()
        };

        return category;
    }

    public void UpdateDetails(string? name = null, string? description = null, string? color = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }

        Description = description?.Trim();

        if (!string.IsNullOrWhiteSpace(color))
        {
            Color = NormalizeColor(color);
        }

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

    private static string NormalizeColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return "#6366f1";

        var normalized = color.Trim();

        // Add # if missing
        if (!normalized.StartsWith('#'))
            normalized = $"#{normalized}";

        // Validate hex color format
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$"))
            return "#6366f1"; // Return default if invalid

        return normalized.ToLowerInvariant();
    }

    public int GetActiveJobsCount()
    {
        return Jobs.Count(j => j.Status == Enums.JobStatus.Active);
    }

    public int GetTotalJobsCount()
    {
        return Jobs.Count(j => j.Status != Enums.JobStatus.Deleted);
    }

    public double GetAverageSuccessRate()
    {
        var activeJobs = Jobs.Where(j => j.Status != Enums.JobStatus.Deleted && j.TotalRuns > 0).ToList();

        if (!activeJobs.Any())
            return 0.0;

        return activeJobs.Average(j => j.GetSuccessRate());
    }

    // Predefined color palette
    public static readonly Dictionary<string, string> ColorPalette = new()
    {
        { "blue", "#3b82f6" },
        { "indigo", "#6366f1" },
        { "purple", "#8b5cf6" },
        { "pink", "#ec4899" },
        { "red", "#ef4444" },
        { "orange", "#f97316" },
        { "yellow", "#eab308" },
        { "green", "#22c55e" },
        { "emerald", "#10b981" },
        { "teal", "#14b8a6" },
        { "cyan", "#06b6d4" },
        { "sky", "#0ea5e9" },
        { "slate", "#64748b" },
        { "gray", "#6b7280" }
    };

    public static string GetColorByName(string colorName)
    {
        return ColorPalette.TryGetValue(colorName.ToLowerInvariant(), out var color)
            ? color
            : "#6366f1";
    }
}