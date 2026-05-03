using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CleanArch.Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.Authentication.Jwt;

/// <summary>
/// JWT token generation and validation service.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IOptions<JwtSettings> settings, ILogger<JwtTokenService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public string GenerateAccessToken(string userId, string email, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public (string UserId, string Email)? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) 
                      ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var email = principal.FindFirstValue(ClaimTypes.Email) 
                     ?? principal.FindFirstValue(JwtRegisteredClaimNames.Email);

            return userId is not null && email is not null ? (userId, email) : null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            throw; // Re-throw unexpected errors
        }
    }
}
