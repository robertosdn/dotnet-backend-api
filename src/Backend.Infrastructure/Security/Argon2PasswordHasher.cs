using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Backend.Application.Abstractions.Security;
using Konscious.Security.Cryptography;

namespace Backend.Infrastructure.Security;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    public async Task<string> HashAsync(string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var salt = RandomNumberGenerator.GetBytes(16);
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 2,
            MemorySize = 64 * 1024,
            Iterations = 3
        };

        var hash = await argon2.GetBytesAsync(32);
        return $"$argon2id$v=19$m=65536,t=3,p=2${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public async Task<bool> VerifyAsync(string password, string passwordHash, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split('$');
        if (parts.Length != 6 || parts[1] != "argon2id" || parts[2] != "v=19")
        {
            return false;
        }

        var parameters = parts[3].Split(',');
        var memoryParam = parameters.FirstOrDefault(p => p.StartsWith("m=", StringComparison.Ordinal));
        var iterationsParam = parameters.FirstOrDefault(p => p.StartsWith("t=", StringComparison.Ordinal));
        var parallelismParam = parameters.FirstOrDefault(p => p.StartsWith("p=", StringComparison.Ordinal));

        if (memoryParam == null || iterationsParam == null || parallelismParam == null)
        {
            return false;
        }

        var memorySize = int.Parse(memoryParam[2..], CultureInfo.InvariantCulture);
        var iterations = int.Parse(iterationsParam[2..], CultureInfo.InvariantCulture);
        var parallelism = int.Parse(parallelismParam[2..], CultureInfo.InvariantCulture);

        var salt = Convert.FromBase64String(parts[4]);
        var expectedHash = Convert.FromBase64String(parts[5]);

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = parallelism,
            MemorySize = memorySize,
            Iterations = iterations
        };

        var computedHash = await argon2.GetBytesAsync(expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
    }
}