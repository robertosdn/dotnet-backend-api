using Backend.Application.AccessUsers.Commands.UpdateAccessUser;
using Backend.Contracts.AccessUsers;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Endpoints.AccessUsers;

public static class UpdateAccessUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{id:guid}", HandleAsync)
            .WithName("UpdateAccessUser")
            .Produces<AccessUserResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status412PreconditionFailed)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateAccessUserRequest request,
        UpdateAccessUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new UpdateAccessUserCommand(id, request.Version, request.Name, request.Email, request.Status, request.Password),
            cancellationToken);

        return result.ErrorCode switch
        {
            null when result.Response is not null => Results.Ok(result.Response),
            "validation" => Results.ValidationProblem(
                result.Errors!,
                title: "The request is invalid."),
            "not_found" => Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Access user not found.",
                Detail = $"An access user with id '{id}' does not exist."
            }),
            "concurrency_conflict" => Results.Problem(
                statusCode: StatusCodes.Status412PreconditionFailed,
                title: "Concurrency conflict.",
                detail: "The resource was modified by another request. Please retry with the latest version."),
            "duplicate_email" => Results.Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email already registered.",
                Detail = "An access user with this email already exists."
            }),
            _ => Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Could not update access user.")
        };
    }
}