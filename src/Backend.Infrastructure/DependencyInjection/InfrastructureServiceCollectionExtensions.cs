using Backend.Application.Abstractions.Persistence;
using Backend.Application.Abstractions.Security;
using Backend.Infrastructure.Persistence.MySql;
using Backend.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        return services;
    }
}