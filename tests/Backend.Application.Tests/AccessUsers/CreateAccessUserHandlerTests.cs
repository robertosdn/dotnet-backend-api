using System.Text.Json;
using Backend.Application.Abstractions.Persistence;
using Backend.Application.Abstractions.Security;
using Backend.Application.AccessUsers.Commands.CreateAccessUser;
using Backend.Domain.AccessUsers;
using Xunit;

namespace Backend.Application.Tests.AccessUsers;

public sealed class CreateAccessUserHandlerTests
{
    [Fact]
    public async Task CreatesUserWithNormalizedEmailAndPublicResponse()
    {
        var repository = new FakeRepository();
        var handler = new CreateAccessUserHandler(repository, new FakePasswordHasher());

        var result = await handler.HandleAsync(
            new CreateAccessUserCommand(" Alice Silva ", " USER@example.com ", "Strong1!"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Response);
        Assert.Equal("user@example.com", result.Response.Email);
        Assert.Equal("Alice Silva", result.Response.Name);
        Assert.DoesNotContain("password_hash", JsonSerializer.Serialize(result.Response));
        Assert.Equal("$argon2id$test", repository.User?.PasswordHash);
        Assert.NotNull(repository.Event);
        Assert.Equal("user@example.com", repository.Event?.Email);
    }

    [Fact]
    public async Task RejectsInvalidPayloadWithoutHashingOrPersisting()
    {
        var repository = new FakeRepository();
        var hasher = new FakePasswordHasher();
        var handler = new CreateAccessUserHandler(repository, hasher);

        var result = await handler.HandleAsync(
            new CreateAccessUserCommand("", "invalid", "weak"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("validation", result.ErrorCode);
        Assert.Contains("name", result.Errors.Keys);
        Assert.Contains("email", result.Errors.Keys);
        Assert.Contains("password", result.Errors.Keys);
        Assert.False(hasher.WasCalled);
        Assert.Null(repository.User);
    }

    [Fact]
    public async Task ReturnsDuplicateEmailErrorFromRepository()
    {
        var repository = new FakeRepository { SaveError = SaveAccessUserError.DuplicateEmail };
        var handler = new CreateAccessUserHandler(repository, new FakePasswordHasher());

        var result = await handler.HandleAsync(
            new CreateAccessUserCommand("Alice Silva", "user@example.com", "Strong1!"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("duplicate_email", result.ErrorCode);
    }

    private sealed class FakeRepository : IAccessUserWriteRepository
    {
        public SaveAccessUserError SaveError { get; init; }
        public AccessUser? User { get; private set; }
        public AccessUserCreatedEvent? Event { get; private set; }

        public Task<SaveAccessUserError> SaveAsync(
            AccessUser user,
            AccessUserCreatedEvent createdEvent,
            CancellationToken cancellationToken)
        {
            User = user;
            Event = createdEvent;
            return Task.FromResult(SaveError);
        }

        public Task<SaveAccessUserError> UpdateAsync(
            AccessUser currentUser,
            AccessUser updatedUser,
            AccessUserUpdatedEvent? updatedEvent,
            long expectedVersion,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(SaveAccessUserError.Storage);
        }

        public Task<AccessUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult<AccessUser?>(null);
        }

        public Task<AccessUser?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult<AccessUser?>(null);
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public bool WasCalled { get; private set; }

        public Task<string> HashAsync(string password, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult("$argon2id$test");
        }

        public Task<bool> VerifyAsync(string password, string passwordHash, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }
    }
}