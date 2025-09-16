using CronCaps.Domain.Enums;

namespace CronCaps.Domain.Exceptions;

public sealed class InvalidJobOperationException : DomainException
{
    public Guid JobId { get; }
    public JobStatus CurrentStatus { get; }
    public string Operation { get; }

    public InvalidJobOperationException(Guid jobId, JobStatus currentStatus, string operation)
        : base("INVALID_JOB_OPERATION", $"Cannot perform operation '{operation}' on job '{jobId}' with status '{currentStatus}'")
    {
        JobId = jobId;
        CurrentStatus = currentStatus;
        Operation = operation;
    }
}