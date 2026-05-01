namespace CleanArch.Presentation.Contracts.Routes;

/// <summary>
/// Centralized API route constants — used by controllers and integration tests.
/// Eliminates magic strings across the codebase.
/// </summary>
public static class ApiRoutes
{
    private const string Base = "api/v{version:apiVersion}";

    // Add your feature routes here:
    //
    // public static class Products
    // {
    //     private const string Controller = $"{Base}/products";
    //     public const string GetAll = Controller;
    //     public const string GetById = $"{Controller}/{{id}}";
    //     public const string Create = Controller;
    //     public const string Update = $"{Controller}/{{id}}";
    //     public const string Delete = $"{Controller}/{{id}}";
    // }
    //
    // public static class Auth
    // {
    //     private const string Controller = $"{Base}/auth";
    //     public const string Login = $"{Controller}/login";
    //     public const string Register = $"{Controller}/register";
    //     public const string RefreshToken = $"{Controller}/refresh";
    // }

    public static class Health
    {
        public const string Check = "/health";
    }
}
