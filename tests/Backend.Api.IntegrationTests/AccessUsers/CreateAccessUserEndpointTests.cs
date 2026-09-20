using System.Net;
using System.Net.Http.Json;
using Backend.Application.Abstractions.Persistence;
using Backend.Application.Abstractions.Security;
using Backend.Domain.AccessUsers;
using Backend.Contracts.AccessUsers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Backend.Api.IntegrationTests.AccessUsers;

public sealed class CreateAccessUserEndpointTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient client;

    public CreateAccessUserEndpointTests(ApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task CreatesUserAndDoesNotReturnPasswordHash()
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/access-users",
            new { name = "Alice Silva", email = "USER@example.com", password = "Strong1!" });

        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains("\"email\":\"user@example.com\"", body);
        Assert.DoesNotContain("password_hash", body);
        Assert.DoesNotContain("Strong1!", body);
    }

    [Fact]
    public async Task RejectsInvalidPayloadWithValidationProblemDetails()
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/access-users",
            new { name = "", email = "invalid", password = "weak" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    internal sealed class FakeRepository : IAccessUserWriteRepository
    {
        public Task<SaveAccessUserError> SaveAsync(
            AccessUser user,
            AccessUserCreatedEvent createdEvent,
            CancellationToken cancellationToken) => Task.FromResult(SaveAccessUserError.None);

        public Task<SaveAccessUserError> UpdateAsync(
            AccessUser currentUser,
            AccessUser updatedUser,
            AccessUserUpdatedEvent? updatedEvent,
            long expectedVersion,
            CancellationToken cancellationToken) => Task.FromResult(SaveAccessUserError.Storage);

        public Task<AccessUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<AccessUser?>(null);

        public Task<AccessUser?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult<AccessUser?>(null);
    }

    internal sealed class FakePasswordHasher : IPasswordHasher
    {
        public Task<string> HashAsync(string password, CancellationToken cancellationToken) =>
            Task.FromResult("$argon2id$integration-test");

        public Task<bool> VerifyAsync(string password, string passwordHash, CancellationToken cancellationToken) =>
            Task.FromResult(false);
    }
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAccessUserWriteRepository>();
            services.RemoveAll<IPasswordHasher>();
            services.AddSingleton<IAccessUserWriteRepository, CreateAccessUserEndpointTests.FakeRepository>();
            services.AddSingleton<IPasswordHasher, CreateAccessUserEndpointTests.FakePasswordHasher>();
        });
    }
}