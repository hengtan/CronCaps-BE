namespace CronCaps.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IJobRepository Jobs { get; }
    IRepository<Entities.JobExecution> JobExecutions { get; }
    IRepository<Entities.Team> Teams { get; }
    IRepository<Entities.Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}