using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;

namespace Mumii.Auth.Infrastructure.Services;

/// <summary>
/// Implementation của IJwtService
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // Chỉ lấy từ environment variable hoặc appsettings - KHÔNG có fallback token
        var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                     _configuration["Jwt:Key"];
        
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT_SECRET_KEY là bắt buộc! Vui lòng cấu hình trong file .env hoặc appsettings.json");
        }
        
        // Validate key length for security (theo hướng dẫn bithub.vn)
        if (jwtKey.Length < 32)
        {
            throw new InvalidOperationException("JWT Secret Key phải có ít nhất 32 ký tự (256 bits) để đảm bảo bảo mật");
        }
        
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    }

    /// <summary>
    /// Generate access token
    /// </summary>
    public string GenerateAccessToken(Account account)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id),
            new(ClaimTypes.Email, account.Email),
            new(ClaimTypes.Name, account.DisplayName),
            new(ClaimTypes.Role, account.Role.ToString()),
            new("account_id", account.Id),
            new("is_active", account.IsActive.ToString()),
            // Thêm claims theo chuẩn JWT (theo hướng dẫn bithub.vn)
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, 
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(GetAccessTokenExpiry()),
            SigningCredentials = credentials,
            Issuer = _configuration["Jwt:Issuer"] ?? "Mumii",
            Audience = _configuration["Jwt:Audience"] ?? "Mumii.Client"
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generate refresh token
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Validate và parse access token
    /// </summary>
    public bool ValidateToken(string token, out string? accountId)
    {
        accountId = null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "Mumii",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "Mumii.Client",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                accountId = principal.FindFirst("account_id")?.Value ??
                           principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return !string.IsNullOrEmpty(accountId);
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get account ID từ token
    /// </summary>
    public string? GetAccountIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);
            
            return jwt.Claims.FirstOrDefault(c => c.Type == "account_id")?.Value ??
                   jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get access token expiry time
    /// </summary>
    private TimeSpan GetAccessTokenExpiry()
    {
        var expiryHours = _configuration.GetValue<int>("Jwt:ExpiryHours");
        return expiryHours > 0 ? TimeSpan.FromHours(expiryHours) : TimeSpan.FromHours(24);
    }
}
