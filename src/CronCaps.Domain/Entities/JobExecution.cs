using CronCaps.Domain.Enums;

namespace CronCaps.Domain.Entities;

public class JobExecution : BaseEntity
{
    public Guid JobId { get; private set; }
    public string ExecutionId { get; private set; } = null!;
    public ExecutionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public long? DurationMs { get; private set; }
    public string? Output { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public string? ServerName { get; private set; }
    public Dictionary<string, object>? Metadata { get; private set; }

    // Navigation properties
    public Job Job { get; private set; } = null!;

    private JobExecution() { } // For EF Core

    private JobExecution(Guid jobId, string executionId)
    {
        JobId = jobId;
        ExecutionId = executionId;
        Status = ExecutionStatus.Pending;
        StartedAt = DateTime.UtcNow;
        RetryCount = 0;
        ServerName = Environment.MachineName;
    }

    public static JobExecution Create(Guid jobId, string? customExecutionId = null)
    {
        var executionId = customExecutionId ?? $"exec_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
        return new JobExecution(jobId, executionId);
    }

    public void Start()
    {
        if (Status != ExecutionStatus.Pending)
            throw new InvalidOperationException("Execution can only be started from Pending status");

        Status = ExecutionStatus.Running;
        StartedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Complete(string? output = null, Dictionary<string, object>? metadata = null)
    {
        if (Status != ExecutionStatus.Running)
            throw new InvalidOperationException("Execution can only be completed from Running status");

        Status = ExecutionStatus.Completed;
        FinishedAt = DateTime.UtcNow;
        Output = output?.Trim();
        Metadata = metadata;
        CalculateDuration();
        UpdateTimestamp();
    }

    public void Fail(string errorMessage, string? output = null, Dictionary<string, object>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty", nameof(errorMessage));

        Status = ExecutionStatus.Failed;
        FinishedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage.Trim();
        Output = output?.Trim();
        Metadata = metadata;
        CalculateDuration();
        UpdateTimestamp();
    }

    public void Cancel(string? reason = null)
    {
        if (Status == ExecutionStatus.Completed || Status == ExecutionStatus.Failed)
            throw new InvalidOperationException("Cannot cancel a completed or failed execution");

        Status = ExecutionStatus.Cancelled;
        FinishedAt = DateTime.UtcNow;
        ErrorMessage = reason?.Trim() ?? "Execution was cancelled";
        CalculateDuration();
        UpdateTimestamp();
    }

    public void Timeout(string? additionalInfo = null)
    {
        if (Status == ExecutionStatus.Completed || Status == ExecutionStatus.Failed)
            throw new InvalidOperationException("Cannot timeout a completed or failed execution");

        Status = ExecutionStatus.Timeout;
        FinishedAt = DateTime.UtcNow;
        ErrorMessage = $"Execution timed out{(!string.IsNullOrEmpty(additionalInfo) ? $": {additionalInfo}" : "")}";
        CalculateDuration();
        UpdateTimestamp();
    }

    public void Skip(string reason)
    {
        if (Status != ExecutionStatus.Pending)
            throw new InvalidOperationException("Execution can only be skipped from Pending status");

        Status = ExecutionStatus.Skipped;
        FinishedAt = DateTime.UtcNow;
        ErrorMessage = reason.Trim();
        CalculateDuration();
        UpdateTimestamp();
    }

    public void IncrementRetryCount()
    {
        RetryCount++;
        UpdateTimestamp();
    }

    public void UpdateProgress(string progressMessage, Dictionary<string, object>? metadata = null)
    {
        if (Status != ExecutionStatus.Running)
            return;

        Output = progressMessage.Trim();
        if (metadata != null)
        {
            Metadata = Metadata != null
                ? new Dictionary<string, object>(Metadata.Concat(metadata))
                : metadata;
        }
        UpdateTimestamp();
    }

    public void SetMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
        UpdateTimestamp();
    }

    public T? GetMetadata<T>(string key)
    {
        if (Metadata == null || !Metadata.TryGetValue(key, out var value))
            return default;

        if (value is T typedValue)
            return typedValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    private void CalculateDuration()
    {
        if (FinishedAt.HasValue)
        {
            DurationMs = (long)(FinishedAt.Value - StartedAt).TotalMilliseconds;
        }
    }

    public bool IsRunning => Status == ExecutionStatus.Running;
    public bool IsCompleted => Status is ExecutionStatus.Completed or ExecutionStatus.Failed or ExecutionStatus.Cancelled or ExecutionStatus.Timeout or ExecutionStatus.Skipped;
    public bool IsSuccessful => Status == ExecutionStatus.Completed;
    public bool HasFailed => Status is ExecutionStatus.Failed or ExecutionStatus.Timeout;

    public TimeSpan? GetDuration()
    {
        return DurationMs.HasValue ? TimeSpan.FromMilliseconds(DurationMs.Value) : null;
    }

    public string GetStatusDescription()
    {
        return Status switch
        {
            ExecutionStatus.Pending => "Waiting to start",
            ExecutionStatus.Running => "Currently running",
            ExecutionStatus.Completed => "Completed successfully",
            ExecutionStatus.Failed => $"Failed: {ErrorMessage}",
            ExecutionStatus.Cancelled => $"Cancelled: {ErrorMessage}",
            ExecutionStatus.Timeout => $"Timed out: {ErrorMessage}",
            ExecutionStatus.Skipped => $"Skipped: {ErrorMessage}",
            _ => "Unknown status"
        };
    }

    public double GetExecutionTimeInSeconds()
    {
        return DurationMs.HasValue ? DurationMs.Value / 1000.0 : 0;
    }

    public bool IsLongRunning(TimeSpan threshold)
    {
        return GetDuration() > threshold;
    }
}