using Backend.Application.AccessUsers.Commands.CreateAccessUser;
using Backend.Contracts.AccessUsers;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Endpoints.AccessUsers;

public static class CreateAccessUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("", HandleAsync)
            .WithName("CreateAccessUser")
            .Produces<AccessUserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        CreateAccessUserRequest request,
        CreateAccessUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new CreateAccessUserCommand(request.Name, request.Email, request.Password),
            cancellationToken);

        return result.ErrorCode switch
        {
            null when result.Response is not null => Results.Created(
                $"/api/v1/access-users/{result.Response.Id}",
                result.Response),
            "validation" => Results.ValidationProblem(
                result.Errors,
                title: "The request is invalid."),
            "duplicate_email" => Results.Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email already registered.",
                Detail = "An access user with this email already exists."
            }),
            _ => Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Could not create access user.")
        };
    }
}