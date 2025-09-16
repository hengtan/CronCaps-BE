namespace CronCaps.Domain.Enums;

public enum ExecutionStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Timeout = 5,
    Skipped = 6
}