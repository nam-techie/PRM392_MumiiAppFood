namespace Mumii.Shared.Common.DTOs;

/// <summary>
/// DTO cho đăng ký tài khoản mới
/// </summary>
public record RegisterRequest(
    string Email,
    string Password,
    string DisplayName
);

/// <summary>
/// DTO cho đăng nhập
/// </summary>
public record LoginRequest(
    string Email,
    string Password
);

/// <summary>
/// DTO cho response đăng nhập thành công
/// </summary>
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    AccountDto Account
);

/// <summary>
/// DTO cho refresh token
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken
);

/// <summary>
/// DTO cho thông tin tài khoản
/// </summary>
public record AccountDto(
    string Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);

/// <summary>
/// DTO cho cập nhật profile
/// </summary>
public record UpdateProfileRequest(
    string DisplayName,
    string? AvatarUrl
);

/// <summary>
/// DTO cho đổi mật khẩu
/// </summary>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
