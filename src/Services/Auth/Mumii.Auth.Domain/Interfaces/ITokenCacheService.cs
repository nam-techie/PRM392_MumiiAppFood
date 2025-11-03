namespace Mumii.Auth.Domain.Interfaces;

/// <summary>
/// Service để quản lý cache token (refresh token, OTP, etc.)
/// </summary>
public interface ITokenCacheService
{
    /// <summary>
    /// Lưu refresh token cho user
    /// </summary>
    Task SaveRefreshTokenAsync(int userId, string refreshToken, TimeSpan expiry);
    
    /// <summary>
    /// Lấy refresh token của user
    /// </summary>
    Task<string?> GetRefreshTokenAsync(int userId);
    
    /// <summary>
    /// Xóa refresh token của user
    /// </summary>
    Task DeleteRefreshTokenAsync(int userId);
    
    /// <summary>
    /// Lưu OTP cho email
    /// </summary>
    Task SaveOtpAsync(string email, string otp, TimeSpan expiry);
    
    /// <summary>
    /// Lấy OTP của email
    /// </summary>
    Task<string?> GetOtpAsync(string email);
    
    /// <summary>
    /// Xóa OTP của email
    /// </summary>
    Task DeleteOtpAsync(string email);
}