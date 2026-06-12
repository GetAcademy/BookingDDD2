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
        object? parameters = null)
    {
        var (connection, transaction) = await GetSessionAsync();

        return await connection.QuerySingleOrDefaultAsync<T>(
            sql,
            parameters,
            transaction);
    }

    internal async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null)
    {
        var (connection, transaction) = await GetSessionAsync();

        var rows = await connection.QueryAsync<T>(
            sql,
            parameters,
            transaction);

        return rows.AsList();
    }

    internal async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null)
    {
        var (connection, transaction) = await GetSessionAsync();

        return await connection.ExecuteAsync(
            sql,
            parameters,
            transaction);
    }

    private async Task<(SqlConnection Connection, DbTransaction Transaction)>
        GetSessionAsync()
    {
        if (_completed)
        {
            throw new InvalidOperationException(
                "This unit of work has already completed.");
        }

        if (_connection is null)
        {
            _connection = new SqlConnection(_options.ConnectionString);
            await _connection.OpenAsync();
            _transaction = await _connection.BeginTransactionAsync();
        }

        return (_connection, _transaction!);
    }

    public async Task CommitAsync()
    {
        if (_completed)
        {
            throw new InvalidOperationException(
                "This unit of work has already completed.");
        }

        if (_transaction is not null)
        {
            await _transaction.CommitAsync();
        }

        _completed = true;
        await DisposeSessionAsync();
    }

    public async Task RollbackAsync()
    {
        if (_completed)
        {
            return;
        }

        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
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
