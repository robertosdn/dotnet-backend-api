using Backend.Application.AccessUsers.Commands.CreateAccessUser;
using Backend.Application.AccessUsers.Commands.UpdateAccessUser;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateAccessUserHandler>();
        services.AddScoped<UpdateAccessUserHandler>();
        return services;
    }
}