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
    private readonly IJwtService _jwtService;
    private readonly IMongoIdGenerator _idGenerator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IJwtService jwtService,
        IMongoIdGenerator idGenerator,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _idGenerator = idGenerator;
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
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Trong thực tế, cần validate refresh token từ database
            // Để đơn giản, ở đây chỉ generate token mới
            return Ok(ApiResponse<LoginResponse>.ErrorResult(
                "Chưa triển khai",
                "Refresh token chưa được triển khai"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, ApiResponse<LoginResponse>.ErrorResult(
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
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        // Trong thực tế, cần blacklist token hoặc xóa refresh token
        // Để đơn giản, chỉ trả về success
        await Task.CompletedTask;
        return Ok(ApiResponse.SuccessResult("Đăng xuất thành công"));
    }
}
