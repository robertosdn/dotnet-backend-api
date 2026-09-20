namespace Backend.Application.Abstractions.Security;

public interface ITokenService
{
    string GenerateToken(Guid userId, string email, string name, TimeSpan expiration);
}