using System.Text.RegularExpressions;

namespace CronCaps.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!IsValid(normalizedEmail))
            throw new ArgumentException($"Invalid email format: {email}", nameof(email));

        return new Email(normalizedEmail);
    }

    public static bool IsValid(string email)
    {
        return !string.IsNullOrWhiteSpace(email) &&
               email.Length <= 254 &&
               EmailRegex.IsMatch(email);
    }

    public string GetDomain()
    {
        var atIndex = Value.IndexOf('@');
        return atIndex > 0 ? Value.Substring(atIndex + 1) : string.Empty;
    }

    public string GetLocalPart()
    {
        var atIndex = Value.IndexOf('@');
        return atIndex > 0 ? Value.Substring(0, atIndex) : string.Empty;
    }

    public override string ToString() => Value;

    // Implicit conversion from string
    public static implicit operator string(Email email) => email.Value;
}