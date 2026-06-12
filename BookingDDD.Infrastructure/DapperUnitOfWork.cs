using System.Data.Common;
using BookingDDD.Core.Abstractions;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BookingDDD.Infrastructure;

public sealed class DapperUnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly SqlServerOptions _options;
    private SqlConnection? _connection;
    private DbTransaction? _transaction;
    private bool _completed;

    public DapperUnitOfWork(SqlServerOptions options)
    {
        _options = options;
    }

    internal async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var (connection, transaction) =
            await GetSessionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<T>(
            new CommandDefinition(
                sql,
                parameters,
                transaction,
                cancellationToken: cancellationToken));
    }

    internal async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var (connection, transaction) =
            await GetSessionAsync(cancellationToken);

        var rows = await connection.QueryAsync<T>(
            new CommandDefinition(
                sql,
                parameters,
                transaction,
                cancellationToken: cancellationToken));

        return rows.AsList();
    }

    internal async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var (connection, transaction) =
            await GetSessionAsync(cancellationToken);

        return await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                parameters,
                transaction,
                cancellationToken: cancellationToken));
    }

    private async Task<(SqlConnection Connection, DbTransaction Transaction)>
        GetSessionAsync(CancellationToken cancellationToken)
    {
        if (_completed)
        {
            throw new InvalidOperationException(
                "This unit of work has already completed.");
        }

        if (_connection is null)
        {
            _connection = new SqlConnection(_options.ConnectionString);
            await _connection.OpenAsync(cancellationToken);
            _transaction = await _connection.BeginTransactionAsync(
                cancellationToken);
        }

        return (_connection, _transaction!);
    }

    public async Task CommitAsync(
        CancellationToken cancellationToken = default)
    {
        if (_completed)
        {
            throw new InvalidOperationException(
                "This unit of work has already completed.");
        }

        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
        }

        _completed = true;
        await DisposeSessionAsync();
    }

    public async Task RollbackAsync(
        CancellationToken cancellationToken = default)
    {
        if (_completed)
        {
            return;
        }

        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
        }

        _completed = true;
        await DisposeSessionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (!_completed)
        {
            await RollbackAsync();
        }

        await DisposeSessionAsync();
    }

    private async Task DisposeSessionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
