namespace Backend.Application.Abstractions.Security;

public interface IPasswordHasher
{
    Task<string> HashAsync(string password, CancellationToken cancellationToken);
    Task<bool> VerifyAsync(string password, string passwordHash, CancellationToken cancellationToken);
}