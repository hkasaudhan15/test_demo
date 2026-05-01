namespace CleanArch.Application.Abstractions.Data;

/// <summary>
/// Read-only DB connection factory for Dapper queries.
/// Bypasses EF Core for performance-critical read paths.
/// </summary>
public interface IDbConnectionFactory
{
    System.Data.IDbConnection CreateConnection();
}
