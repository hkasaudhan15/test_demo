using System.Data;
using CleanArch.Application.Abstractions.Data;
using Microsoft.Data.SqlClient;

namespace CleanArch.Infrastructure.Persistence.EFCore.Repositories;

/// <summary>
/// SQL Server connection factory for Dapper queries.
/// </summary>
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
