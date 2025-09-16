using CronCaps.Application.DTOs;
using CronCaps.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace CronCaps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<AnalyticsDto>> GetOverallAnalytics(CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await _analyticsService.GetOverallAnalyticsAsync(cancellationToken);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overall analytics");
            return StatusCode(500, "An error occurred while retrieving analytics");
        }
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<AnalyticsDto>> GetAnalyticsByUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await _analyticsService.GetAnalyticsByUserAsync(userId, cancellationToken);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user analytics");
        }
    }

    [HttpGet("team/{teamId:guid}")]
    public async Task<ActionResult<AnalyticsDto>> GetAnalyticsByTeam(
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await _analyticsService.GetAnalyticsByTeamAsync(teamId, cancellationToken);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics for team {TeamId}", teamId);
            return StatusCode(500, "An error occurred while retrieving team analytics");
        }
    }

    [HttpGet("system")]
    public async Task<ActionResult<SystemMetricsDto>> GetSystemMetrics(CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await _analyticsService.GetSystemMetricsAsync(cancellationToken);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system metrics");
            return StatusCode(500, "An error occurred while retrieving system metrics");
        }
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats(
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _analyticsService.GetDashboardStatsAsync(userId, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return StatusCode(500, "An error occurred while retrieving dashboard stats");
        }
    }

    [HttpGet("trends")]
    public async Task<ActionResult<IEnumerable<DailyExecutionStatsDto>>> GetExecutionTrends(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (endDate <= startDate)
            {
                return BadRequest("End date must be after start date");
            }

            if ((endDate - startDate).TotalDays > 90)
            {
                return BadRequest("Date range cannot exceed 90 days");
            }

            var trends = await _analyticsService.GetExecutionTrendsAsync(startDate, endDate, cancellationToken);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution trends from {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, "An error occurred while retrieving execution trends");
        }
    }

    [HttpGet("health")]
    public async Task<ActionResult<IEnumerable<JobHealthDto>>> GetUnhealthyJobs(CancellationToken cancellationToken = default)
    {
        try
        {
            var unhealthyJobs = await _analyticsService.GetUnhealthyJobsAsync(cancellationToken);
            return Ok(unhealthyJobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unhealthy jobs");
            return StatusCode(500, "An error occurred while retrieving unhealthy jobs");
        }
    }
}