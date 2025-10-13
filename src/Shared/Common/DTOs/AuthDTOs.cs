namespace Mumii.Shared.Common.DTOs;

/// <summary>
/// DTO cho đăng ký user mới
/// </summary>
public record RegisterRequest(
    string Email,
    string Password,
    string Fullname
);

/// <summary>
/// DTO cho đăng nhập
/// </summary>
public record LoginRequest(
    string Email,
    string Password
);

/// <summary>
/// DTO cho đăng nhập với Google
/// </summary>
public record GoogleLoginRequest(
    string GoogleToken
);

/// <summary>
/// DTO cho response đăng nhập thành công
/// </summary>
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    UserDto User
);

/// <summary>
/// DTO cho refresh token
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken
);

/// <summary>
/// DTO cho thông tin user
/// </summary>
public record UserDto(
    int Id,
    string Email,
    string Fullname,
    string Role,
    bool IsActive,
    string LoginMethod,
    DateTime CreatedAt,
    ProfileDto? Profile
);

/// <summary>
/// DTO cho profile người dùng
/// </summary>
public record ProfileDto(
    int Id,
    int UserId,
    string? Gender,
    string? Avatar,
    string? PhoneNumber,
    string? Address,
    DateTime CreatedAt
);

/// <summary>
/// DTO cho notification
/// </summary>
public record NotificationDto(
    int Id,
    int UserId,
    string Title,
    string Content,
    bool IsRead,
    DateTime CreatedAt
);

/// <summary>
/// DTO cho cập nhật profile
/// </summary>
public record UpdateProfileRequest(
    string? Gender,
    string? Avatar,
    string? PhoneNumber,
    string? Address
);

/// <summary>
/// DTO cho cập nhật thông tin cơ bản
/// </summary>
public record UpdateUserRequest(
    string Fullname
);

/// <summary>
/// DTO cho đổi mật khẩu
/// </summary>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

/// <summary>
/// DTO cho tạo notification
/// </summary>
public record CreateNotificationRequest(
    string Title,
    string Content
);

// Giữ lại AccountDto để backward compatibility
/// <summary>
/// DTO cho thông tin tài khoản (legacy)
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
