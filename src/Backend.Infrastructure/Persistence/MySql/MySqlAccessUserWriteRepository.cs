using System.Text.Json;
using Backend.Application.Abstractions.Persistence;
using Backend.Domain.AccessUsers;
using MySqlConnector;

namespace Backend.Infrastructure.Persistence.MySql;

public sealed class MySqlAccessUserWriteRepository(MySqlDataSource dataSource) : IAccessUserWriteRepository
{
    public async Task<SaveAccessUserError> SaveAsync(
        AccessUser user,
        AccessUserCreatedEvent createdEvent,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await InsertUserAsync(connection, transaction, user, cancellationToken);
            await InsertOutboxEventAsync(connection, transaction, createdEvent, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return SaveAccessUserError.None;
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            return SaveAccessUserError.DuplicateEmail;
        }
        catch (MySqlException)
        {
            return SaveAccessUserError.Storage;
        }
    }

    private static async Task InsertUserAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        AccessUser user,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO access_users
                (id, email, name, password_hash, status, version, created_at, updated_at)
            VALUES
                (@id, @email, @name, @password_hash, 'active', @version, @created_at, @updated_at)
            """;
        command.Parameters.Add("@id", MySqlDbType.Binary, 16).Value = user.Id.ToByteArray();
        command.Parameters.AddWithValue("@email", user.Email.Value);
        command.Parameters.AddWithValue("@name", user.Name.Value);
        command.Parameters.AddWithValue("@password_hash", user.PasswordHash);
        command.Parameters.AddWithValue("@version", user.Version);
        command.Parameters.AddWithValue("@created_at", user.CreatedAt);
        command.Parameters.AddWithValue("@updated_at", user.UpdatedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertOutboxEventAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        AccessUserCreatedEvent createdEvent,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            event_id = createdEvent.EventId,
            aggregate_id = createdEvent.AggregateId,
            email = createdEvent.Email,
            name = createdEvent.Name,
            status = createdEvent.Status.ToString().ToLowerInvariant(),
            version = createdEvent.Version,
            created_at = createdEvent.CreatedAt,
            occurred_at = createdEvent.OccurredAt
        });

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO access_users_outbox
                (id, aggregate_id, event_type, payload, status, attempts, available_at, created_at)
            VALUES
                (@id, @aggregate_id, 'AccessUserCreated', @payload, 'pending', 0, @available_at, @created_at)
            """;
        command.Parameters.Add("@id", MySqlDbType.Binary, 16).Value = createdEvent.EventId.ToByteArray();
        command.Parameters.Add("@aggregate_id", MySqlDbType.Binary, 16).Value = createdEvent.AggregateId.ToByteArray();
        command.Parameters.AddWithValue("@payload", payload);
        command.Parameters.AddWithValue("@available_at", createdEvent.OccurredAt);
        command.Parameters.AddWithValue("@created_at", createdEvent.OccurredAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}