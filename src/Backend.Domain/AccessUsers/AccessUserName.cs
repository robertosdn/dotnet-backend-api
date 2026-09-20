using Backend.Domain.Common;

namespace Backend.Domain.AccessUsers;

public sealed record AccessUserName
{
    public string Value { get; }

    private AccessUserName(string value) => Value = value;

    public static AccessUserName Parse(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length is 0 or > 255)
        {
            throw new DomainValidationException("invalid name");
        }

        return new AccessUserName(normalized);
    }

    public override string ToString() => Value;
}