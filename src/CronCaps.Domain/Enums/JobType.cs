namespace CronCaps.Domain.Enums;

public enum JobType
{
    HttpRequest = 1,
    Command = 2,
    Script = 3,
    StoredProcedure = 4,
    EmailAlert = 5,
    DataSync = 6,
    FileProcess = 7,
    Custom = 99
}