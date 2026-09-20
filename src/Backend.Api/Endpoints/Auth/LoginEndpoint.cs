using Backend.Application.Auth.Commands.Login;
using Backend.Contracts.Auth;

namespace Backend.Api.Endpoints.Auth;

public static class LoginEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/login", HandleAsync)
            .WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        LoginRequest request,
        LoginHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new LoginCommand(request.Email!, request.Password!),
            cancellationToken);

        return result.ErrorCode switch
        {
            null when result.Response is not null => Results.Ok(result.Response),
            "validation" => Results.ValidationProblem(
                result.Errors,
                title: "The request is invalid."),
            "invalid_credentials" => Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid credentials.",
                detail: "The provided email or password is incorrect."),
            "user_disabled" => Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Account disabled.",
                detail: "This account has been disabled."),
            _ => Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Could not process login.")
        };
    }
}