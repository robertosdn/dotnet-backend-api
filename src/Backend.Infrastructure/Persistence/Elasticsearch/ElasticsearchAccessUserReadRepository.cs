using Backend.Application.Abstractions.Persistence;
using Backend.Contracts.AccessUsers;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Persistence.Elasticsearch;

public sealed class ElasticsearchAccessUserReadRepository(
    ElasticsearchClient client,
    IOptions<ElasticsearchOptions> options) : IAccessUserReadRepository
{
    private readonly string _indexName = options.Value.AccessUsersIndexName;

    public async Task<AccessUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync<AccessUserDocument>(
            id.ToString(),
            g => g.Index(_indexName),
            cancellationToken);

        if (!response.Found || response.Source is null)
        {
            return null;
        }

        var doc = response.Source;
        return new AccessUserResponse(
            doc.Id,
            doc.Email,
            doc.Name,
            doc.Status,
            doc.Version,
            doc.CreatedAt,
            doc.UpdatedAt);
    }

    private sealed record AccessUserDocument(
        Guid Id,
        string Email,
        string Name,
        string Status,
        long Version,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}