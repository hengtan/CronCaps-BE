namespace CronCaps.Application.DTOs;

public record AnalyticsDto(
    int TotalJobs,
    int ActiveJobs,
    int PausedJobs,
    int FailedJobs,
    int TotalExecutions,
    int SuccessfulExecutions,
    int FailedExecutions,
    double OverallSuccessRate,
    double AverageExecutionTime,
    List<JobCategoryStatsDto> CategoryStats,
    List<DailyExecutionStatsDto> DailyStats,
    List<TopJobDto> TopJobsByExecution,
    List<JobHealthDto> UnhealthyJobs
);

public record JobCategoryStatsDto(
    string CategoryName,
    int JobCount,
    int ExecutionCount,
    double SuccessRate
);

public record DailyExecutionStatsDto(
    DateOnly Date,
    int TotalExecutions,
    int SuccessfulExecutions,
    int FailedExecutions,
    double SuccessRate
);

public record TopJobDto(
    Guid JobId,
    string JobName,
    int ExecutionCount,
    double SuccessRate,
    double AverageExecutionTime
);

public record JobHealthDto(
    Guid JobId,
    string JobName,
    int RecentFailures,
    DateTime LastFailureAt,
    string? LastErrorMessage,
    double FailureRate
);

public record SystemMetricsDto(
    double CpuUsage,
    long MemoryUsage,
    long TotalMemory,
    int ActiveConnections,
    int QueuedJobs,
    DateTime LastUpdated
);

public record DashboardStatsDto(
    AnalyticsDto Analytics,
    SystemMetricsDto SystemMetrics,
    List<JobDto> RecentJobs,
    List<JobExecutionDto> RecentExecutions,
    List<string> SystemAlerts
);