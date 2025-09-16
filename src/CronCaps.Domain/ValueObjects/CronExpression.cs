using System.Text.RegularExpressions;

namespace CronCaps.Domain.ValueObjects;

public sealed record CronExpression
{
    public string Value { get; }

    private CronExpression(string value)
    {
        Value = value;
    }

    public static CronExpression Create(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Cron expression cannot be null or empty", nameof(expression));

        var normalizedExpression = expression.Trim();

        if (!IsValidCronExpression(normalizedExpression))
            throw new ArgumentException($"Invalid cron expression: {expression}", nameof(expression));

        return new CronExpression(normalizedExpression);
    }

    public static bool IsValidCronExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        // Basic validation for 5-field or 6-field cron expressions
        // Format: [second] minute hour day month day-of-week [year]
        var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length is 5 or 6 &&
               parts.All(part => IsValidCronField(part));
    }

    private static bool IsValidCronField(string field)
    {
        // Allow numbers, ranges (1-5), lists (1,2,3), steps (*/5), and special characters
        var cronFieldPattern = @"^(\*|(\d+(-\d+)?(,\d+(-\d+)?)*))(/\d+)?$|^(\*|\?|L|W|#)$";
        return Regex.IsMatch(field, cronFieldPattern);
    }

    public DateTime? GetNextExecution(DateTime fromTime)
    {
        // This would integrate with a cron parsing library like NCrontab or Cronos
        // For now, returning a simple example
        return fromTime.AddMinutes(5); // Placeholder implementation
    }

    public bool ShouldExecuteAt(DateTime dateTime)
    {
        // Implementation would check if the given datetime matches the cron expression
        return true; // Placeholder implementation
    }

    public override string ToString() => Value;

    // Implicit conversion from string
    public static implicit operator string(CronExpression cronExpression) => cronExpression.Value;

    // Common predefined expressions
    public static CronExpression EveryMinute => Create("* * * * *");
    public static CronExpression Hourly => Create("0 * * * *");
    public static CronExpression Daily => Create("0 0 * * *");
    public static CronExpression Weekly => Create("0 0 * * 0");
    public static CronExpression Monthly => Create("0 0 1 * *");
}