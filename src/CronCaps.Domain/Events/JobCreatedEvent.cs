namespace CronCaps.Domain.Events;

public sealed record JobCreatedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid JobId { get; init; }
    public string JobName { get; init; }
    public string CronExpression { get; init; }
    public Guid CreatedById { get; init; }
    public Guid? TeamId { get; init; }

    public JobCreatedEvent(Guid jobId, string jobName, string cronExpression, Guid createdById, Guid? teamId = null)
    {
        JobId = jobId;
        JobName = jobName;
        CronExpression = cronExpression;
        CreatedById = createdById;
        TeamId = teamId;
    }
}