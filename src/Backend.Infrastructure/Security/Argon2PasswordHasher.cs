using System.Security.Cryptography;
using Backend.Application.Abstractions.Security;
using Konscious.Security.Cryptography;

namespace Backend.Infrastructure.Security;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    public async Task<string> HashAsync(string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var salt = RandomNumberGenerator.GetBytes(16);
        var argon2 = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 2,
            MemorySize = 64 * 1024,
            Iterations = 3
        };

        var hash = await argon2.GetBytesAsync(32);
        return $"$argon2id$v=19$m=65536,t=3,p=2${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
}