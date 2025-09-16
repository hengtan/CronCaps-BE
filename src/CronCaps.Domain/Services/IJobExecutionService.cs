using CronCaps.Domain.Entities;
using CronCaps.Domain.Enums;

namespace CronCaps.Domain.Services;

public interface IJobExecutionService
{
    Task<JobExecution> CreateExecution(Job job);
    Task<bool> CanJobExecuteConcurrently(Job job);
    Task<bool> HasRunningExecution(Guid jobId);
    Task<ExecutionStatus> DetermineExecutionStatus(JobExecution execution);
    Task<bool> ShouldRetryExecution(JobExecution execution);
    Task<TimeSpan> CalculateRetryDelay(JobExecution execution, int retryAttempt);
    Task NotifyExecutionCompleted(JobExecution execution);
    Task HandleExecutionTimeout(JobExecution execution);
}