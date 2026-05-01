namespace CleanArch.Application.Abstractions.Authentication;

/// <summary>
/// JWT token service abstraction.
/// Application defines the contract; Infrastructure implements JWT logic.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(string userId, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
    (string UserId, string Email)? ValidateToken(string token);
}
