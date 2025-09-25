using Mumii.Auth.Domain.Entities;

namespace Mumii.Auth.Domain.Interfaces;

/// <summary>
/// Service interface cho JWT token generation
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate access token
    /// </summary>
    string GenerateAccessToken(Account account);

    /// <summary>
    /// Generate refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate và parse access token
    /// </summary>
    bool ValidateToken(string token, out string? accountId);

    /// <summary>
    /// Get account ID từ token
    /// </summary>
    string? GetAccountIdFromToken(string token);
}
