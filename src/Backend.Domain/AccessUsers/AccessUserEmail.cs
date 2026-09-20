using Backend.Domain.Common;

namespace Backend.Domain.AccessUsers;

public sealed record AccessUserEmail
{
    public string Value { get; }

    private AccessUserEmail(string value) => Value = value;

    public static AccessUserEmail Parse(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;
        var at = normalized.IndexOf('@');
        var dot = normalized.LastIndexOf('.');

        if (normalized.Length > 254 || at <= 0 || dot <= at + 1 || dot >= normalized.Length - 1 || normalized.Any(char.IsWhiteSpace))
        {
            throw new DomainValidationException("invalid email");
        }

        return new AccessUserEmail(normalized);
    }

    public override string ToString() => Value;
}