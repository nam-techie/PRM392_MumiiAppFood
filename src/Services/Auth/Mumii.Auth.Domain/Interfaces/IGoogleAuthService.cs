namespace Mumii.Auth.Domain.Interfaces;

/// <summary>
/// Interface cho Google authentication service
/// </summary>
public interface IGoogleAuthService
{
    /// <summary>
    /// Verify Google ID token và trả về thông tin user
    /// </summary>
    /// <param name="idToken">Google ID token</param>
    /// <returns>Thông tin user từ Google hoặc null nếu invalid</returns>
    Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken);
}

/// <summary>
/// Thông tin user từ Google
/// </summary>
/// <param name="Email">Email từ Google</param>
/// <param name="Name">Tên từ Google</param>
/// <param name="GoogleId">Google ID</param>
/// <param name="Picture">URL ảnh đại diện</param>
public record GoogleUserInfo(
    string Email, 
    string Name, 
    string GoogleId, 
    string? Picture
);
