namespace Backend.Api.Endpoints.Auth;

public static class AuthEndpointGroup
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication");

        LoginEndpoint.Map(group);
    }
}