using Backend.Application.Abstractions.Persistence;
using Backend.Application.Abstractions.Security;
using Backend.Infrastructure.Persistence.Elasticsearch;
using Backend.Infrastructure.Persistence.MySql;
using Backend.Infrastructure.Security;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Backend.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("ConnectionStrings:MySql is required.");

        services.AddSingleton(new MySqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<IAccessUserWriteRepository, MySqlAccessUserWriteRepository>();
        services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();

        services.Configure<JwtTokenService.JwtOptions>(options => configuration.GetSection(JwtTokenService.JwtOptions.SectionName).Bind(options));
        services.AddScoped<ITokenService, JwtTokenService>();

        var elasticsearchOptions = new ElasticsearchOptions();
        configuration.GetSection(ElasticsearchOptions.SectionName).Bind(elasticsearchOptions);
        services.AddSingleton(elasticsearchOptions);
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<ElasticsearchOptions>();
            var settings = new ElasticsearchClientSettings(new Uri(options.Url));
            return new ElasticsearchClient(settings);
        });
        services.AddScoped<IAccessUserReadRepository, ElasticsearchAccessUserReadRepository>();

        return services;
    }
}