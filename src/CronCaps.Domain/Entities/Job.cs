using CronCaps.Domain.Enums;
using CronCaps.Domain.ValueObjects;

namespace CronCaps.Domain.Entities;

public class Job : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public CronExpression CronExpression { get; private set; } = null!;
    public JobConfiguration Configuration { get; private set; } = null!;
    public JobStatus Status { get; private set; }
    public Guid CreatedById { get; private set; }
    public Guid? TeamId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public DateTime? NextRunAt { get; private set; }
    public DateTime? LastRunAt { get; private set; }
    public int TotalRuns { get; private set; }
    public int SuccessfulRuns { get; private set; }
    public int FailedRuns { get; private set; }
    public double? AverageExecutionTimeMs { get; private set; }
    public string? Tags { get; private set; }

    // Navigation properties
    public User CreatedBy { get; private set; } = null!;
    public Team? Team { get; private set; }
    public Category? Category { get; private set; }
    public ICollection<JobExecution> Executions { get; private set; } = new List<JobExecution>();

    private Job() { } // For EF Core

    private Job(string name, CronExpression cronExpression, JobConfiguration configuration, Guid createdById)
    {
        Name = name;
        CronExpression = cronExpression;
        Configuration = configuration;
        CreatedById = createdById;
        Status = JobStatus.Draft;
        TotalRuns = 0;
        SuccessfulRuns = 0;
        FailedRuns = 0;
        CalculateNextRun();
    }

    public static Job Create(
        string name,
        string cronExpression,
        JobConfiguration configuration,
        Guid createdById,
        string? description = null,
        Guid? teamId = null,
        Guid? categoryId = null,
        string? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Job name cannot be null or empty", nameof(name));

        var job = new Job(name, CronExpression.Create(cronExpression), configuration, createdById)
        {
            Description = description?.Trim(),
            TeamId = teamId,
            CategoryId = categoryId,
            Tags = tags?.Trim()
        };

        return job;
    }

    public void UpdateDetails(
        string? name = null,
        string? description = null,
        string? cronExpression = null,
        string? tags = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }

        Description = description?.Trim();
        Tags = tags?.Trim();

        if (!string.IsNullOrWhiteSpace(cronExpression) && cronExpression != CronExpression.Value)
        {
            CronExpression = CronExpression.Create(cronExpression);
            CalculateNextRun();
        }

        UpdateTimestamp();
    }

    public void UpdateConfiguration(JobConfiguration newConfiguration)
    {
        Configuration = newConfiguration;
        UpdateTimestamp();
    }

    public void Activate()
    {
        if (Status == JobStatus.Active) return;

        Status = JobStatus.Active;
        CalculateNextRun();
        UpdateTimestamp();
    }

    public void Pause()
    {
        if (Status == JobStatus.Paused) return;

        Status = JobStatus.Paused;
        NextRunAt = null;
        UpdateTimestamp();
    }

    public void Resume()
    {
        if (Status != JobStatus.Paused) return;

        Status = JobStatus.Active;
        CalculateNextRun();
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        Status = JobStatus.Inactive;
        NextRunAt = null;
        UpdateTimestamp();
    }

    public void MarkAsError(string? errorMessage = null)
    {
        Status = JobStatus.Error;
        NextRunAt = null;
        UpdateTimestamp();
    }

    public void Delete()
    {
        Status = JobStatus.Deleted;
        NextRunAt = null;
        UpdateTimestamp();
    }

    public void AssignToTeam(Guid? teamId)
    {
        TeamId = teamId;
        UpdateTimestamp();
    }

    public void AssignToCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        UpdateTimestamp();
    }

    public bool CanExecute()
    {
        return Status == JobStatus.Active &&
               NextRunAt.HasValue &&
               NextRunAt <= DateTime.UtcNow;
    }

    public bool ShouldExecuteNow()
    {
        return CanExecute() && CronExpression.ShouldExecuteAt(DateTime.UtcNow);
    }

    public void RecordExecution(JobExecution execution)
    {
        TotalRuns++;
        LastRunAt = execution.StartedAt;

        if (execution.Status == ExecutionStatus.Completed)
        {
            SuccessfulRuns++;
        }
        else if (execution.Status == ExecutionStatus.Failed)
        {
            FailedRuns++;
        }

        UpdateAverageExecutionTime(execution.DurationMs);
        CalculateNextRun();
        UpdateTimestamp();
    }

    private void UpdateAverageExecutionTime(long? durationMs)
    {
        if (!durationMs.HasValue) return;

        if (AverageExecutionTimeMs.HasValue)
        {
            // Update running average
            var totalSuccessfulRuns = SuccessfulRuns;
            if (totalSuccessfulRuns > 0)
            {
                AverageExecutionTimeMs = ((AverageExecutionTimeMs * (totalSuccessfulRuns - 1)) + durationMs) / totalSuccessfulRuns;
            }
        }
        else
        {
            AverageExecutionTimeMs = durationMs;
        }
    }

    private void CalculateNextRun()
    {
        if (Status != JobStatus.Active)
        {
            NextRunAt = null;
            return;
        }

        var now = DateTime.UtcNow;
        NextRunAt = CronExpression.GetNextExecution(now);
    }

    public double GetSuccessRate()
    {
        if (TotalRuns == 0) return 0.0;
        return (double)SuccessfulRuns / TotalRuns * 100;
    }

    public double GetFailureRate()
    {
        if (TotalRuns == 0) return 0.0;
        return (double)FailedRuns / TotalRuns * 100;
    }

    public bool IsHealthy()
    {
        if (TotalRuns < 5) return true; // Too few runs to determine health

        var recentFailures = Executions
            .Where(e => e.StartedAt > DateTime.UtcNow.AddDays(-1))
            .Count(e => e.Status == ExecutionStatus.Failed);

        return recentFailures < 3; // Unhealthy if more than 3 failures in the last 24 hours
    }

    public TimeSpan? GetEstimatedDuration()
    {
        return AverageExecutionTimeMs.HasValue
            ? TimeSpan.FromMilliseconds(AverageExecutionTimeMs.Value)
            : null;
    }

    public IEnumerable<string> GetTagsList()
    {
        if (string.IsNullOrWhiteSpace(Tags))
            return Enumerable.Empty<string>();

        return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(tag => tag.Trim())
                  .Where(tag => !string.IsNullOrEmpty(tag));
    }

    public bool HasTag(string tag)
    {
        return GetTagsList().Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
    }
}