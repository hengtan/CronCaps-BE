using CronCaps.Domain.Entities;
using CronCaps.Domain.Enums;

namespace CronCaps.Domain.Interfaces;

public interface IJobRepository : IRepository<Job>
{
    Task<IEnumerable<Job>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> GetByStatusAsync(JobStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> GetJobsDueForExecutionAsync(DateTime asOfTime, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> GetActiveJobsAsync(CancellationToken cancellationToken = default);
    Task<Job?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> SearchJobsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> GetJobsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> GetJobsByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeJobId = null, CancellationToken cancellationToken = default);
}