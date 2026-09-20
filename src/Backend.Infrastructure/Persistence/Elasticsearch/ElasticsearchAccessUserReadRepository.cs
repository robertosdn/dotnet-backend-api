using Backend.Application.Abstractions.Persistence;
using Backend.Contracts.AccessUsers;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
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

    public async Task<(IReadOnlyList<AccessUserResponse> Items, long TotalCount)> SearchAsync(
        string? status,
        string? email,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var from = (page - 1) * pageSize;
        var filters = new List<Query>();

        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add(Query.Term(new TermQuery(new Field("status")) { Value = status }));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            filters.Add(Query.Term(new TermQuery(new Field("email")) { Value = email }));
        }

        Query query = filters.Count == 0
            ? Query.MatchAll(new MatchAllQuery())
            : Query.Bool(new BoolQuery { Filter = filters });

        var response = await client.SearchAsync<AccessUserDocument>(
            _indexName,
            s => s
                .From(from)
                .Size(pageSize)
                .Query(query)
                .Sort(so => so
                    .Field(new Field("created_at"), new FieldSort { Order = SortOrder.Asc })
                    .Field(new Field("id"), new FieldSort { Order = SortOrder.Asc })),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException("Could not search access users in Elasticsearch.");
        }

        var items = response.Documents
            .Select(doc => new AccessUserResponse(
                doc.Id,
                doc.Email,
                doc.Name,
                doc.Status,
                doc.Version,
                doc.CreatedAt,
                doc.UpdatedAt))
            .ToList();

        return (items, response.Total);
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