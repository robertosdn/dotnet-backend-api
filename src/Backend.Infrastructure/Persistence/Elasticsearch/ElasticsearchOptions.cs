namespace Backend.Infrastructure.Persistence.Elasticsearch;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Url { get; init; } = "http://localhost:9200";
    public string AccessUsersIndexName { get; init; } = "access_users";
}