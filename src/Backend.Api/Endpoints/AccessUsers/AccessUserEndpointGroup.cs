namespace Backend.Api.Endpoints.AccessUsers;

public static class AccessUserEndpointGroup
{
    public static void MapAccessUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/access-users")
            .WithTags("Access Users");

        CreateAccessUserEndpoint.Map(group);
        UpdateAccessUserEndpoint.Map(group);
        GetAccessUserByIdEndpoint.Map(group);
    }
}