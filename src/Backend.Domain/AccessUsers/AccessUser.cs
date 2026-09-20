using Backend.Domain.Common;

namespace Backend.Domain.AccessUsers;

public sealed class AccessUser
{
    private AccessUser(Guid id, AccessUserEmail email, AccessUserName name, string passwordHash, DateTime now)
    {
        Id = id;
        Email = email;
        Name = name;
        PasswordHash = passwordHash;
        Status = AccessUserStatus.Active;
        Version = 1;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; }
    public AccessUserEmail Email { get; }
    public AccessUserName Name { get; }
    public string PasswordHash { get; }
    public AccessUserStatus Status { get; }
    public long Version { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    public static AccessUser Create(Guid id, AccessUserEmail email, AccessUserName name, string passwordHash, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException("password hash is required");
        }

        return new AccessUser(id, email, name, passwordHash, now);
    }

    public AccessUserCreatedEvent CreateCreatedEvent()
    {
        return new AccessUserCreatedEvent(
            Guid.NewGuid(),
            Id,
            Email.Value,
            Name.Value,
            Status,
            Version,
            CreatedAt,
            UpdatedAt);
    }
}