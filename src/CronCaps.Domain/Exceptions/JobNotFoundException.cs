namespace CronCaps.Domain.Exceptions;

public sealed class JobNotFoundException : DomainException
{
    public Guid JobId { get; }

    public JobNotFoundException(Guid jobId)
        : base("JOB_NOT_FOUND", $"Job with ID '{jobId}' was not found")
    {
        JobId = jobId;
    }
}