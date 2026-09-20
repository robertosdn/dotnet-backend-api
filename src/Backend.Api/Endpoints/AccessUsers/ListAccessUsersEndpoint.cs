using Backend.Application.AccessUsers.Queries.ListAccessUsers;
using Backend.Contracts.AccessUsers;

namespace Backend.Api.Endpoints.AccessUsers;

public static class ListAccessUsersEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", HandleAsync)
            .WithName("ListAccessUsers")
            .Produces<ListAccessUsersResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        ListAccessUsersHandler handler,
        int page = 1,
        int pageSize = 20,
        string? status = null,
        string? email = null,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.HandleAsync(
            new ListAccessUsersQuery(page, pageSize, status, email),
            cancellationToken);

        return result.ErrorCode switch
        {
            null when result.Response is not null => Results.Ok(result.Response),
            "validation" => Results.ValidationProblem(
                result.Errors,
                title: "The request is invalid."),
            _ => Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Could not list access users.")
        };
    }
}
