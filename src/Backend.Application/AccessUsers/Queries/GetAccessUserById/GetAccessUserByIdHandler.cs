using Backend.Application.Abstractions.Persistence;

namespace Backend.Application.AccessUsers.Queries.GetAccessUserById;

public sealed class GetAccessUserByIdHandler(IAccessUserReadRepository repository)
{
    public async Task<GetAccessUserByIdResult> HandleAsync(
        GetAccessUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        var response = await repository.GetByIdAsync(query.Id, cancellationToken);

        return response is not null
            ? GetAccessUserByIdResult.Success(response)
            : GetAccessUserByIdResult.NotFound();
    }
}