namespace Backend.Application.Abstractions.Security;

public interface IPasswordHasher
{
    Task<string> HashAsync(string password, CancellationToken cancellationToken);
}