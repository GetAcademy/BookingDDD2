using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BookingDDD.Infrastructure;

public sealed class SqlAuditLog(SqlServerOptions options) : IAuditLog
{
    public async Task RecordAsync(
        string eventName,
        BookingId bookingId,
        ResourceId resourceId,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO dbo.AuditLogEntries
                (Id, EventName, BookingId, ResourceId, OccurredAtUtc)
            VALUES
                (@Id, @EventName, @BookingId, @ResourceId, @OccurredAtUtc);
            """;

        await using var connection =
            new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = Guid.NewGuid(),
                EventName = eventName,
                BookingId = bookingId.Value,
                ResourceId = resourceId.Value,
                OccurredAtUtc = occurredAtUtc
            },
            cancellationToken: cancellationToken));
    }
}
