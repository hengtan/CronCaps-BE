using CronCaps.Domain.Enums;

namespace CronCaps.Domain.Events;

public sealed record JobExecutionCompletedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid JobId { get; init; }
    public Guid ExecutionId { get; init; }
    public ExecutionStatus Status { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime FinishedAt { get; init; }
    public long DurationMs { get; init; }
    public string? Output { get; init; }
    public string? ErrorMessage { get; init; }
    public int RetryCount { get; init; }

    public JobExecutionCompletedEvent(
        Guid jobId,
        Guid executionId,
        ExecutionStatus status,
        DateTime startedAt,
        DateTime finishedAt,
        long durationMs,
        string? output = null,
        string? errorMessage = null,
        int retryCount = 0)
    {
        JobId = jobId;
        ExecutionId = executionId;
        Status = status;
        StartedAt = startedAt;
        FinishedAt = finishedAt;
        DurationMs = durationMs;
        Output = output;
        ErrorMessage = errorMessage;
        RetryCount = retryCount;
    }
}