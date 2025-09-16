using System.Text.Json;
using CronCaps.Domain.Enums;

namespace CronCaps.Domain.ValueObjects;

public sealed record JobConfiguration
{
    public JobType Type { get; }
    public Dictionary<string, object> Parameters { get; }
    public TimeSpan? Timeout { get; }
    public int MaxRetries { get; }
    public bool AllowConcurrent { get; }
    public string? NotificationEmail { get; }

    private JobConfiguration(
        JobType type,
        Dictionary<string, object> parameters,
        TimeSpan? timeout,
        int maxRetries,
        bool allowConcurrent,
        string? notificationEmail)
    {
        Type = type;
        Parameters = parameters;
        Timeout = timeout;
        MaxRetries = maxRetries;
        AllowConcurrent = allowConcurrent;
        NotificationEmail = notificationEmail;
    }

    public static JobConfiguration Create(
        JobType type,
        Dictionary<string, object>? parameters = null,
        TimeSpan? timeout = null,
        int maxRetries = 0,
        bool allowConcurrent = false,
        string? notificationEmail = null)
    {
        if (maxRetries < 0)
            throw new ArgumentException("Max retries cannot be negative", nameof(maxRetries));

        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero)
            throw new ArgumentException("Timeout must be positive", nameof(timeout));

        if (!string.IsNullOrEmpty(notificationEmail) && !Email.IsValid(notificationEmail))
            throw new ArgumentException("Invalid notification email", nameof(notificationEmail));

        return new JobConfiguration(
            type,
            parameters ?? new Dictionary<string, object>(),
            timeout,
            maxRetries,
            allowConcurrent,
            notificationEmail);
    }

    public T? GetParameter<T>(string key)
    {
        if (!Parameters.TryGetValue(key, out var value))
            return default;

        if (value is T typedValue)
            return typedValue;

        if (value is JsonElement jsonElement)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }
            catch
            {
                return default;
            }
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    public bool HasParameter(string key) => Parameters.ContainsKey(key);

    public JobConfiguration WithParameter(string key, object value)
    {
        var newParameters = new Dictionary<string, object>(Parameters)
        {
            [key] = value
        };

        return new JobConfiguration(Type, newParameters, Timeout, MaxRetries, AllowConcurrent, NotificationEmail);
    }

    public JobConfiguration WithTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentException("Timeout must be positive", nameof(timeout));

        return new JobConfiguration(Type, Parameters, timeout, MaxRetries, AllowConcurrent, NotificationEmail);
    }

    // Factory methods for common job types
    public static JobConfiguration CreateHttpRequest(
        string url,
        string method = "GET",
        Dictionary<string, string>? headers = null,
        string? body = null,
        TimeSpan? timeout = null)
    {
        var parameters = new Dictionary<string, object>
        {
            ["url"] = url,
            ["method"] = method.ToUpperInvariant()
        };

        if (headers?.Any() == true)
            parameters["headers"] = headers;

        if (!string.IsNullOrEmpty(body))
            parameters["body"] = body;

        return Create(JobType.HttpRequest, parameters, timeout ?? TimeSpan.FromMinutes(5));
    }

    public static JobConfiguration CreateCommand(
        string command,
        string? workingDirectory = null,
        Dictionary<string, string>? environmentVariables = null,
        TimeSpan? timeout = null)
    {
        var parameters = new Dictionary<string, object>
        {
            ["command"] = command
        };

        if (!string.IsNullOrEmpty(workingDirectory))
            parameters["workingDirectory"] = workingDirectory;

        if (environmentVariables?.Any() == true)
            parameters["environmentVariables"] = environmentVariables;

        return Create(JobType.Command, parameters, timeout ?? TimeSpan.FromMinutes(30));
    }

    public static JobConfiguration CreateScript(
        string scriptPath,
        string? interpreter = null,
        string[]? arguments = null,
        TimeSpan? timeout = null)
    {
        var parameters = new Dictionary<string, object>
        {
            ["scriptPath"] = scriptPath
        };

        if (!string.IsNullOrEmpty(interpreter))
            parameters["interpreter"] = interpreter;

        if (arguments?.Any() == true)
            parameters["arguments"] = arguments;

        return Create(JobType.Script, parameters, timeout ?? TimeSpan.FromMinutes(30));
    }
}