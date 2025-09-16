using CronCaps.Domain.Entities;

namespace CronCaps.Domain.Services;

public interface IJobSchedulerService
{
    Task<DateTime?> CalculateNextExecution(Job job);
    Task<bool> ShouldJobExecuteNow(Job job);
    Task<IEnumerable<Job>> GetJobsDueForExecution(DateTime? asOfTime = null);
    Task<bool> IsValidCronExpression(string cronExpression);
    Task<IEnumerable<DateTime>> GetNextExecutions(string cronExpression, int count = 5);
    Task<string> GetCronDescription(string cronExpression);
}