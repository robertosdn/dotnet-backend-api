using Backend.Application.AccessUsers.Queries.GetAccessUserById;
using Backend.Contracts.AccessUsers;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Endpoints.AccessUsers;

public static class GetAccessUserByIdEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", HandleAsync)
            .WithName("GetAccessUserById")
            .Produces<AccessUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        GetAccessUserByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetAccessUserByIdQuery(id),
            cancellationToken);

        return result.ErrorCode switch
        {
            null when result.Response is not null => Results.Ok(result.Response),
            "not_found" => Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Access user not found.",
                Detail = $"An access user with id '{id}' does not exist."
            }),
            _ => Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Could not retrieve access user.")
        };
    }
}