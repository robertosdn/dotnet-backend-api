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

    public async Task<SaveAccessUserError> UpdateAsync(
        AccessUser currentUser,
        AccessUser updatedUser,
        AccessUserUpdatedEvent? updatedEvent,
        long expectedVersion,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var rowsAffected = await UpdateUserAsync(connection, transaction, updatedUser, expectedVersion, cancellationToken);
            if (rowsAffected == 0)
            {
                return SaveAccessUserError.ConcurrencyConflict;
            }

            if (updatedEvent != null)
            {
                await InsertOutboxEventAsync(connection, transaction, updatedEvent, cancellationToken);
            }

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

    public async Task<AccessUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, email, name, password_hash, status, version, created_at, updated_at
            FROM access_users
            WHERE id = @id
            """;
        command.Parameters.Add("@id", MySqlDbType.Binary, 16).Value = id.ToByteArray();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var userId = new Guid(reader.GetFieldValue<byte[]>(0));
        var email = new AccessUserEmail(reader.GetString(1));
        var name = new AccessUserName(reader.GetString(2));
        var passwordHash = reader.GetString(3);
        var status = Enum.Parse<AccessUserStatus>(reader.GetString(4), true);
        var version = reader.GetInt64(5);
        var createdAt = reader.GetDateTime(6);
        var updatedAt = reader.GetDateTime(7);

        return new AccessUser(userId, email, name, passwordHash, status, version, createdAt, updatedAt);
    }

    public async Task<AccessUser?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, email, name, password_hash, status, version, created_at, updated_at
            FROM access_users
            WHERE email = @email
            """;
        command.Parameters.AddWithValue("@email", email);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var userId = new Guid(reader.GetFieldValue<byte[]>(0));
        var emailVo = new AccessUserEmail(reader.GetString(1));
        var name = new AccessUserName(reader.GetString(2));
        var passwordHash = reader.GetString(3);
        var status = Enum.Parse<AccessUserStatus>(reader.GetString(4), true);
        var version = reader.GetInt64(5);
        var createdAt = reader.GetDateTime(6);
        var updatedAt = reader.GetDateTime(7);

        return new AccessUser(userId, emailVo, name, passwordHash, status, version, createdAt, updatedAt);
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

    private static async Task<int> UpdateUserAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        AccessUser user,
        long expectedVersion,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE access_users
            SET email = @email,
                name = @name,
                password_hash = @password_hash,
                status = @status,
                version = @version,
                updated_at = @updated_at
            WHERE id = @id AND version = @expected_version
            """;
        command.Parameters.Add("@id", MySqlDbType.Binary, 16).Value = user.Id.ToByteArray();
        command.Parameters.AddWithValue("@email", user.Email.Value);
        command.Parameters.AddWithValue("@name", user.Name.Value);
        command.Parameters.AddWithValue("@password_hash", user.PasswordHash);
        command.Parameters.AddWithValue("@status", user.Status.ToString().ToLowerInvariant());
        command.Parameters.AddWithValue("@version", user.Version);
        command.Parameters.AddWithValue("@updated_at", user.UpdatedAt);
        command.Parameters.AddWithValue("@expected_version", expectedVersion);
        return await command.ExecuteNonQueryAsync(cancellationToken);
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

    private static async Task InsertOutboxEventAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        AccessUserUpdatedEvent updatedEvent,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            event_id = updatedEvent.EventId,
            aggregate_id = updatedEvent.AggregateId,
            email = updatedEvent.Email,
            name = updatedEvent.Name,
            status = updatedEvent.Status.ToString().ToLowerInvariant(),
            version = updatedEvent.Version,
            updated_at = updatedEvent.UpdatedAt,
            occurred_at = updatedEvent.OccurredAt
        });

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO access_users_outbox
                (id, aggregate_id, event_type, payload, status, attempts, available_at, created_at)
            VALUES
                (@id, @aggregate_id, 'AccessUserUpdated', @payload, 'pending', 0, @available_at, @created_at)
            """;
        command.Parameters.Add("@id", MySqlDbType.Binary, 16).Value = updatedEvent.EventId.ToByteArray();
        command.Parameters.Add("@aggregate_id", MySqlDbType.Binary, 16).Value = updatedEvent.AggregateId.ToByteArray();
        command.Parameters.AddWithValue("@payload", payload);
        command.Parameters.AddWithValue("@available_at", updatedEvent.OccurredAt);
        command.Parameters.AddWithValue("@created_at", updatedEvent.OccurredAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}