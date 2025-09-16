using CronCaps.Domain.Enums;

namespace CronCaps.Application.DTOs;

public record JobDto(
    Guid Id,
    string Name,
    string? Description,
    string CronExpression,
    JobStatus Status,
    Guid CreatedById,
    string CreatedByName,
    Guid? TeamId,
    string? TeamName,
    Guid? CategoryId,
    string? CategoryName,
    DateTime? NextRunAt,
    DateTime? LastRunAt,
    int TotalRuns,
    int SuccessfulRuns,
    int FailedRuns,
    double? AverageExecutionTimeMs,
    string? Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    double SuccessRate,
    double FailureRate,
    bool IsHealthy,
    TimeSpan? EstimatedDuration
);

public record CreateJobDto(
    string Name,
    string? Description,
    string CronExpression,
    Guid? TeamId,
    Guid? CategoryId,
    string? Tags,
    JobConfigurationDto Configuration
);

public record UpdateJobDto(
    string? Name,
    string? Description,
    string? CronExpression,
    string? Tags,
    JobConfigurationDto? Configuration
);

public record JobConfigurationDto(
    int TimeoutSeconds,
    int MaxRetries,
    bool NotifyOnFailure,
    bool NotifyOnSuccess,
    string? NotificationEmail,
    Dictionary<string, string>? Environment
);

public record JobExecutionDto(
    Guid Id,
    Guid JobId,
    string JobName,
    ExecutionStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    long? DurationMs,
    string? Output,
    string? ErrorMessage,
    int RetryCount,
    string? TriggeredBy
);

public record JobExecutionHistoryDto(
    List<JobExecutionDto> Executions,
    int TotalCount,
    int PageNumber,
    int PageSize,
    bool HasNextPage,
    bool HasPreviousPage
);