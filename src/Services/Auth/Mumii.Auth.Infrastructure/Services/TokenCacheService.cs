using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mumii.Auth.Domain.Interfaces;

namespace Mumii.Auth.Infrastructure.Services;

/// <summary>
/// Implementation của ITokenCacheService sử dụng MemoryCache
/// </summary>
public class TokenCacheService : ITokenCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenCacheService> _logger;

    public TokenCacheService(IMemoryCache cache, ILogger<TokenCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task SaveRefreshTokenAsync(int userId, string refreshToken, TimeSpan expiry)
    {
        var key = $"refresh_token_{userId}";
        _cache.Set(key, refreshToken, expiry);
        _logger.LogDebug("Refresh token saved for user {UserId}", userId);
        await Task.CompletedTask;
    }

    public async Task<string?> GetRefreshTokenAsync(int userId)
    {
        var key = $"refresh_token_{userId}";
        var token = _cache.Get<string>(key);
        _logger.LogDebug("Refresh token retrieved for user {UserId}: {Found}", userId, token != null);
        return await Task.FromResult(token);
    }

    public async Task DeleteRefreshTokenAsync(int userId)
    {
        var key = $"refresh_token_{userId}";
        _cache.Remove(key);
        _logger.LogDebug("Refresh token deleted for user {UserId}", userId);
        await Task.CompletedTask;
    }

    public async Task SaveOtpAsync(string email, string otp, TimeSpan expiry)
    {
        var key = $"otp_{email}";
        _cache.Set(key, otp, expiry);
        _logger.LogDebug("OTP saved for email {Email}", email);
        await Task.CompletedTask;
    }

    public async Task<string?> GetOtpAsync(string email)
    {
        var key = $"otp_{email}";
        var otp = _cache.Get<string>(key);
        _logger.LogDebug("OTP retrieved for email {Email}: {Found}", email, otp != null);
        return await Task.FromResult(otp);
    }

    public async Task DeleteOtpAsync(string email)
    {
        var key = $"otp_{email}";
        _cache.Remove(key);
        _logger.LogDebug("OTP deleted for email {Email}", email);
        await Task.CompletedTask;
    }
}
