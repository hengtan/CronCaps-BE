using CronCaps.Application.DTOs;

namespace CronCaps.Application.UseCases;

public interface IAnalyticsService
{
    Task<AnalyticsDto> GetOverallAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<AnalyticsDto> GetAnalyticsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AnalyticsDto> GetAnalyticsByTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<SystemMetricsDto> GetSystemMetricsAsync(CancellationToken cancellationToken = default);
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid? userId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyExecutionStatsDto>> GetExecutionTrendsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobHealthDto>> GetUnhealthyJobsAsync(CancellationToken cancellationToken = default);
}