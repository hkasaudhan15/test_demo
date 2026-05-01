using System.Data;
using CleanArch.Application.Abstractions.Data;
using Microsoft.Data.SqlClient;

namespace CleanArch.Infrastructure.Persistence.EFCore.Repositories;

/// <summary>
/// SQL Server connection factory for Dapper queries.
/// Returns a new unopened connection each call — callers are responsible
/// for opening and disposing via <c>using</c>.
/// </summary>
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
