using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BookingDDD.Infrastructure;

public sealed class SqlBookingCalendar(SqlServerOptions options)
    : IBookingCalendar
{
    public async Task AddAsync(
        BookingId bookingId,
        ResourceId resourceId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.CalendarEntries
            SET ResourceId = @ResourceId,
                StartTime = @StartTime,
                EndTime = @EndTime
            WHERE BookingId = @BookingId;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO dbo.CalendarEntries
                    (BookingId, ResourceId, StartTime, EndTime)
                VALUES
                    (@BookingId, @ResourceId, @StartTime, @EndTime);
            END;
            """;

        await ExecuteAsync(
            sql,
            new
            {
                BookingId = bookingId.Value,
                ResourceId = resourceId.Value,
                StartTime = start,
                EndTime = end
            },
            cancellationToken);
    }

    public Task RemoveAsync(
        BookingId bookingId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            """
            DELETE FROM dbo.CalendarEntries
            WHERE BookingId = @BookingId;
            """,
            new { BookingId = bookingId.Value },
            cancellationToken);

    private async Task ExecuteAsync(
        string sql,
        object parameters,
        CancellationToken cancellationToken)
    {
        await using var connection =
            new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken));
    }
}
