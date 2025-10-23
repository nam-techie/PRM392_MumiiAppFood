using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Services;
using Mumii.Shared.Common.Constants;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;

namespace Mumii.Auth.Api.Controllers;

/// <summary>
/// Controller xử lý authentication và authorization
/// </summary>
[ApiController]
[Route(ApiRoutes.Auth.Base)]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IJwtService _jwtService;
    private readonly IMongoIdGenerator _idGenerator;
    private readonly IEmailService _emailService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IJwtService jwtService,
        IMongoIdGenerator idGenerator,
        IEmailService emailService,
        IGoogleAuthService googleAuthService,
        ICloudinaryService cloudinaryService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _jwtService = jwtService;
        _idGenerator = idGenerator;
        _emailService = emailService;
        _googleAuthService = googleAuthService;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Kiểm tra email đã tồn tại
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult(
                    "Email đã được sử dụng",
                    "Email này đã có tài khoản"));
            }

            // Generate ID cho user mới
            var userId = await _idGenerator.GetNextIdAsync("users", cancellationToken);

            // Tạo user mới
            var newUser = Mumii.Auth.Domain.Entities.User.CreateWithEmail(userId, request.Email, request.Password, request.Fullname);
            await _userRepository.AddAsync(newUser, cancellationToken);

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessTokenForUser(newUser);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var response = new LoginResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                User: new UserDto(
                    newUser.Id,
                    newUser.Email,
                    newUser.Fullname,
                    newUser.Role,
                    newUser.IsActive,
                    newUser.LoginMethod,
                    newUser.CreatedAt,
                    null // Profile - sẽ được tạo riêng
                )
            );

            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return Ok(ApiResponse<LoginResponse>.SuccessResult(response, "Đăng ký thành công"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Registration validation failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<LoginResponse>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return StatusCode(500, ApiResponse<LoginResponse>.ErrorResult(
                "Lỗi hệ thống", 
                "Đã xảy ra lỗi trong quá trình đăng ký"));
        }
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Tìm user
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null || !user.VerifyPassword(request.Password))
            {
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult(
                    "Đăng nhập thất bại",
                    "Email hoặc mật khẩu không đúng"));
            }

            // Kiểm tra user có active không
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult(
                    "Tài khoản bị khóa",
                    "Tài khoản của bạn đã bị vô hiệu hóa"));
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessTokenForUser(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var response = new LoginResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                User: new UserDto(
                    user.Id,
                    user.Email,
                    user.Fullname,
                    user.Role,
                    user.IsActive,
                    user.LoginMethod,
                    user.CreatedAt,
                    null // Profile - sẽ được load riêng nếu cần
                )
            );

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(ApiResponse<LoginResponse>.SuccessResult(response, "Đăng nhập thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, ApiResponse<LoginResponse>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi trong quá trình đăng nhập"));
        }
    }

    /// <summary>
    /// Đăng nhập với Google
    /// </summary>
    [HttpPost("google")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> GoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify Google token
            var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
            if (googleUser == null)
            {
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult(
                    "Xác thực Google thất bại",
                    "Token Google không hợp lệ"));
            }

            // Tìm user theo email
            var existingUser = await _userRepository.GetByEmailAsync(googleUser.Email, cancellationToken);
            
            Mumii.Auth.Domain.Entities.User user;
            if (existingUser != null)
            {
                // User đã tồn tại - login
                user = existingUser;
            }
            else
            {
                // Tạo user mới
                var userId = await _idGenerator.GetNextIdAsync("users", cancellationToken);
                user = Mumii.Auth.Domain.Entities.User.CreateWithGoogle(userId, googleUser.Email, googleUser.Name, googleUser.GoogleId);
                await _userRepository.AddAsync(user, cancellationToken);
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessTokenForUser(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            
            // Set refresh token
            user.SetRefreshToken(refreshToken);
            await _userRepository.UpdateAsync(user, cancellationToken);

            var response = new LoginResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                User: new UserDto(
                    user.Id,
                    user.Email,
                    user.Fullname,
                    user.Role,
                    user.IsActive,
                    user.LoginMethod,
                    user.CreatedAt,
                    null // Profile - sẽ được load riêng
                )
            );

            _logger.LogInformation("Google login successful for user: {Email}", googleUser.Email);
            return Ok(ApiResponse<LoginResponse>.SuccessResult(response, "Đăng nhập Google thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            return StatusCode(500, ApiResponse<LoginResponse>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi trong quá trình đăng nhập Google"));
        }
    }

    /// <summary>
    /// Quên mật khẩu
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Tìm user theo email
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                // Không trả về lỗi để tránh email enumeration attack
                _logger.LogInformation("Forgot password requested for non-existent email: {Email}", request.Email);
                return Ok(ApiResponse.SuccessResult("Nếu email tồn tại, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu"));
            }

            // Generate reset token
            user.GeneratePasswordResetToken();
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Gửi email
            await _emailService.SendPasswordResetEmailAsync(user.Email, user.Fullname, user.PasswordResetToken!);

            _logger.LogInformation("Password reset email sent to: {Email}", request.Email);
            return Ok(ApiResponse.SuccessResult("Nếu email tồn tại, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for email: {Email}", request.Email);
            return StatusCode(500, ApiResponse.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi trong quá trình xử lý yêu cầu"));
        }
    }

    /// <summary>
    /// Đặt lại mật khẩu
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Tìm user theo email
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                return BadRequest(ApiResponse.ErrorResult(
                    "Không tìm thấy tài khoản",
                    "Email không tồn tại trong hệ thống"));
            }

            // Reset password với token
            user.ResetPasswordWithToken(request.Token, request.NewPassword);
            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("Password reset successful for user: {Email}", request.Email);
            return Ok(ApiResponse.SuccessResult("Đặt lại mật khẩu thành công"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse.ErrorResult("Token không hợp lệ", ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for email: {Email}", request.Email);
            return StatusCode(500, ApiResponse.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi trong quá trình đặt lại mật khẩu"));
        }
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Tìm user có refresh token này
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (user == null)
            {
                return BadRequest(ApiResponse<RefreshTokenResponse>.ErrorResult(
                    "Refresh token không hợp lệ",
                    "Token không tồn tại hoặc đã bị thu hồi"));
            }

            // Generate new access token
            var accessToken = _jwtService.GenerateAccessTokenForUser(user);

            var response = new RefreshTokenResponse(AccessToken: accessToken);
            return Ok(ApiResponse<RefreshTokenResponse>.SuccessResult(response, "Refresh token thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, ApiResponse<RefreshTokenResponse>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi trong quá trình refresh token"));
        }
    }

    /// <summary>
    /// Lấy thông tin user hiện tại
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile(CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst("user_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(ApiResponse<UserDto>.ErrorResult(
                    "Không xác thực",
                    "Token không hợp lệ"));
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResult(
                    "Không tìm thấy",
                    "Tài khoản không tồn tại"));
            }

            var userDto = new UserDto(
                user.Id,
                user.Email,
                user.Fullname,
                user.Role,
                user.IsActive,
                user.LoginMethod,
                user.CreatedAt,
                null // Profile - sẽ được load riêng
            );

            return Ok(ApiResponse<UserDto>.SuccessResult(userDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi lấy thông tin user"));
        }
    }

    /// <summary>
    /// Cập nhật thông tin user
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile(
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst("user_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(ApiResponse<UserDto>.ErrorResult(
                    "Không xác thực",
                    "Token không hợp lệ"));
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResult(
                    "Không tìm thấy",
                    "Tài khoản không tồn tại"));
            }

            // Cập nhật basic info
            user.UpdateBasicInfo(request.Fullname);
            await _userRepository.UpdateAsync(user, cancellationToken);

            var userDto = new UserDto(
                user.Id,
                user.Email,
                user.Fullname,
                user.Role,
                user.IsActive,
                user.LoginMethod,
                user.CreatedAt,
                null // Profile
            );

            return Ok(ApiResponse<UserDto>.SuccessResult(userDto, "Cập nhật thông tin thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user info");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi cập nhật thông tin"));
        }
    }

    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst("user_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(ApiResponse.ErrorResult(
                    "Không xác thực",
                    "Token không hợp lệ"));
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResult(
                    "Không tìm thấy",
                    "Tài khoản không tồn tại"));
            }

            // Đổi mật khẩu
            user.ChangePassword(request.CurrentPassword, request.NewPassword);
            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return Ok(ApiResponse.SuccessResult("Đổi mật khẩu thành công"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse.ErrorResult("Mật khẩu không đúng", ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, ApiResponse.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi đổi mật khẩu"));
        }
    }

    /// <summary>
    /// Đăng xuất
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Logout(CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst("user_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(ApiResponse.ErrorResult(
                    "Không xác thực",
                    "Token không hợp lệ"));
            }

            // Clear refresh token
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                user.ClearRefreshToken();
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            _logger.LogInformation("User logged out successfully: {UserId}", userId);
            return Ok(ApiResponse.SuccessResult("Đăng xuất thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, ApiResponse.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi trong quá trình đăng xuất"));
        }
    }

    /// <summary>
    /// Lấy thông tin profile chi tiết
    /// </summary>
    [HttpGet("profile/me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProfileDetailDto>>> GetProfileDetail(CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst("user_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(ApiResponse<ProfileDetailDto>.ErrorResult(
                    "Không xác thực",
                    "Token không hợp lệ"));
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound(ApiResponse<ProfileDetailDto>.ErrorResult(
                    "Không tìm thấy",
                    "Tài khoản không tồn tại"));
            }

            // Load profile
            var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
            var profileDto = profile != null ? new ProfileDto(
                profile.Id,
                profile.UserId,
                profile.Gender,
                profile.Avatar,
                profile.PhoneNumber,
                profile.Address,
                profile.CreatedAt
            ) : null;

            var response = new ProfileDetailDto(
                user.Id,
                user.Email,
                user.Fullname,
                user.Role,
                user.IsActive,
                user.LoginMethod,
                user.CreatedAt,
                profileDto
            );

            return Ok(ApiResponse<ProfileDetailDto>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile detail");
            return StatusCode(500, ApiResponse<ProfileDetailDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi lấy thông tin profile"));
        }
    }

    /// <summary>
    /// Cập nhật profile
    /// </summary>
    [HttpPut("profile/me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProfileDetailDto>>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst("user_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(ApiResponse<ProfileDetailDto>.ErrorResult(
                    "Không xác thực",
                    "Token không hợp lệ"));
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound(ApiResponse<ProfileDetailDto>.ErrorResult(
                    "Không tìm thấy",
                    "Tài khoản không tồn tại"));
            }

            // Update user basic info if provided
            if (!string.IsNullOrWhiteSpace(request.Fullname))
            {
                user.UpdateBasicInfo(request.Fullname);
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            // Load or create profile
            var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
            if (profile == null)
            {
                var profileId = await _idGenerator.GetNextIdAsync("profiles", cancellationToken);
                profile = Profile.Create(profileId, userId, request.Gender, null, request.PhoneNumber, request.Address);
                await _profileRepository.AddAsync(profile, cancellationToken);
            }
            else
            {
                profile.Update(request.Gender, profile.Avatar, request.PhoneNumber, request.Address);
                await _profileRepository.UpdateAsync(profile, cancellationToken);
            }

            var profileDto = new ProfileDto(
                profile.Id,
                profile.UserId,
                profile.Gender,
                profile.Avatar,
                profile.PhoneNumber,
                profile.Address,
                profile.CreatedAt
            );

            var response = new ProfileDetailDto(
                user.Id,
                user.Email,
                user.Fullname,
                user.Role,
                user.IsActive,
                user.LoginMethod,
                user.CreatedAt,
                profileDto
            );

            return Ok(ApiResponse<ProfileDetailDto>.SuccessResult(response, "Cập nhật profile thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ProfileDetailDto>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, ApiResponse<ProfileDetailDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi cập nhật profile"));
        }
    }

    /// <summary>
    /// Upload avatar
    /// </summary>
    [HttpPost("profile/avatar")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UploadAvatarResponse>>> UploadAvatar(
        IFormFile avatar,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst("user_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(ApiResponse<UploadAvatarResponse>.ErrorResult(
                    "Không xác thực",
                    "Token không hợp lệ"));
            }

            if (avatar == null || avatar.Length == 0)
            {
                return BadRequest(ApiResponse<UploadAvatarResponse>.ErrorResult(
                    "File không hợp lệ",
                    "Vui lòng chọn file ảnh"));
            }

            // Upload to Cloudinary
            var fileData = new FormFileAdapter(avatar);
            var avatarUrl = await _cloudinaryService.UploadImageAsync(fileData, "avatars");

            // Update profile
            var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
            if (profile == null)
            {
                var profileId = await _idGenerator.GetNextIdAsync("profiles", cancellationToken);
                profile = Profile.Create(profileId, userId, null, avatarUrl, null, null);
                await _profileRepository.AddAsync(profile, cancellationToken);
            }
            else
            {
                profile.UpdateAvatar(avatarUrl);
                await _profileRepository.UpdateAsync(profile, cancellationToken);
            }

            var response = new UploadAvatarResponse(AvatarUrl: avatarUrl);
            return Ok(ApiResponse<UploadAvatarResponse>.SuccessResult(response, "Upload avatar thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<UploadAvatarResponse>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar");
            return StatusCode(500, ApiResponse<UploadAvatarResponse>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi upload avatar"));
        }
    }
}
