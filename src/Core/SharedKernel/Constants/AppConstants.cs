namespace CleanArch.SharedKernel.Constants;

/// <summary>
/// Application-wide constants. Organized by concern.
/// </summary>
public static class AppConstants
{
    public static class Cache
    {
        public const int DefaultExpirationMinutes = 30;
        public const int ShortExpirationMinutes = 5;
        public const int LongExpirationMinutes = 120;
    }

    public static class Pagination
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
    }

    public static class Auth
    {
        public const string AdminRole = "Admin";
        public const string UserRole = "User";
        public const string ManagerRole = "Manager";
    }

    public static class Headers
    {
        public const string CorrelationId = "X-Correlation-Id";
        public const string TenantId = "X-Tenant-Id";
        public const string IdempotencyKey = "X-Idempotency-Key";
        public const string ApiVersion = "X-Api-Version";
    }
}
