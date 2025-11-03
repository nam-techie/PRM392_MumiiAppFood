using Google.Apis.Auth;
using Mumii.Auth.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Mumii.Auth.Infrastructure.Services;

/// <summary>
/// Service xử lý Google authentication
/// </summary>
public class GoogleAuthService : IGoogleAuthService
{
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly string? _clientId;

    public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
    {
        _logger = logger;
        _clientId = configuration["GoogleAuth:ClientId"];
    }

    /// <summary>
    /// Verify Google ID token và trả về thông tin user
    /// </summary>
    public async Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                _logger.LogWarning("Google ID token is null or empty");
                return null;
            }

            // Verify token với Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

            if (payload == null)
            {
                _logger.LogWarning("Google token verification failed - payload is null");
                return null;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(payload.Email) || 
                string.IsNullOrEmpty(payload.Name) || 
                string.IsNullOrEmpty(payload.Subject))
            {
                _logger.LogWarning("Google token missing required fields: Email={Email}, Name={Name}, Subject={Subject}", 
                    payload.Email, payload.Name, payload.Subject);
                return null;
            }

            var userInfo = new GoogleUserInfo(
                Email: payload.Email,
                Name: payload.Name,
                GoogleId: payload.Subject,
                Picture: payload.Picture
            );

            _logger.LogInformation("Google token verified successfully for user: {Email}", payload.Email);
            return userInfo;
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Google token verification failed - Invalid JWT");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Google token");
            return null;
        }
    }
}
