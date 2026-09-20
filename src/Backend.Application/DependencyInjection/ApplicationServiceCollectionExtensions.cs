using Backend.Application.AccessUsers.Commands.CreateAccessUser;
using Backend.Application.AccessUsers.Commands.UpdateAccessUser;
using Backend.Application.AccessUsers.Queries.GetAccessUserById;
using Backend.Application.Auth.Commands.Login;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateAccessUserHandler>();
        services.AddScoped<UpdateAccessUserHandler>();
        services.AddScoped<GetAccessUserByIdHandler>();
        services.AddScoped<LoginHandler>();
        return services;
    }
}