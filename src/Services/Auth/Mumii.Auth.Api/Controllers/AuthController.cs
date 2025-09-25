using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;
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
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAccountRepository accountRepository,
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _accountRepository = accountRepository;
        _jwtService = jwtService;
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
            var existingAccount = await _accountRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingAccount != null)
            {
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult(
                    "Email đã được sử dụng",
                    "Email này đã có tài khoản"));
            }

            // Tạo tài khoản mới
            var account = Account.Create(request.Email, request.Password, request.DisplayName);
            await _accountRepository.AddAsync(account, cancellationToken);
            await _accountRepository.SaveChangesAsync(cancellationToken);

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(account);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var response = new LoginResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                Account: new AccountDto(
                    account.Id,
                    account.Email,
                    account.DisplayName,
                    account.AvatarUrl,
                    account.Role.ToString(),
                    account.IsActive,
                    account.CreatedAt
                )
            );

            _logger.LogInformation("Account registered successfully: {Email}", request.Email);
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
            // Tìm tài khoản
            var account = await _accountRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (account == null || !account.VerifyPassword(request.Password))
            {
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult(
                    "Đăng nhập thất bại",
                    "Email hoặc mật khẩu không đúng"));
            }

            // Kiểm tra tài khoản có active không
            if (!account.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive account: {Email}", request.Email);
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult(
                    "Tài khoản bị khóa",
                    "Tài khoản của bạn đã bị vô hiệu hóa"));
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(account);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var response = new LoginResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                Account: new AccountDto(
                    account.Id,
                    account.Email,
                    account.DisplayName,
                    account.AvatarUrl,
                    account.Role.ToString(),
                    account.IsActive,
                    account.CreatedAt
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
    /// Lấy thông tin profile hiện tại
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AccountDto>>> GetProfile(CancellationToken cancellationToken)
    {
        try
        {
            var accountId = User.FindFirst("account_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(accountId))
            {
                return Unauthorized(ApiResponse<AccountDto>.ErrorResult(
                    "Không xác thực",
                    "Token không hợp lệ"));
            }

            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
            if (account == null)
            {
                return NotFound(ApiResponse<AccountDto>.ErrorResult(
                    "Không tìm thấy",
                    "Tài khoản không tồn tại"));
            }

            var accountDto = new AccountDto(
                account.Id,
                account.Email,
                account.DisplayName,
                account.AvatarUrl,
                account.Role.ToString(),
                account.IsActive,
                account.CreatedAt
            );

            return Ok(ApiResponse<AccountDto>.SuccessResult(accountDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile");
            return StatusCode(500, ApiResponse<AccountDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi lấy thông tin profile"));
        }
    }

    /// <summary>
    /// Cập nhật profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AccountDto>>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var accountId = User.FindFirst("account_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(accountId))
            {
                return Unauthorized(ApiResponse<AccountDto>.ErrorResult(
                    "Không xác thực",
                    "Token không hợp lệ"));
            }

            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
            if (account == null)
            {
                return NotFound(ApiResponse<AccountDto>.ErrorResult(
                    "Không tìm thấy",
                    "Tài khoản không tồn tại"));
            }

            // Cập nhật profile
            account.UpdateProfile(request.DisplayName, request.AvatarUrl);
            await _accountRepository.UpdateAsync(account, cancellationToken);
            await _accountRepository.SaveChangesAsync(cancellationToken);

            var accountDto = new AccountDto(
                account.Id,
                account.Email,
                account.DisplayName,
                account.AvatarUrl,
                account.Role.ToString(),
                account.IsActive,
                account.CreatedAt
            );

            return Ok(ApiResponse<AccountDto>.SuccessResult(accountDto, "Cập nhật profile thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AccountDto>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, ApiResponse<AccountDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi cập nhật profile"));
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
            var accountId = User.FindFirst("account_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(accountId))
            {
                return Unauthorized(ApiResponse.ErrorResult(
                    "Không xác thực",
                    "Token không hợp lệ"));
            }

            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
            if (account == null)
            {
                return NotFound(ApiResponse.ErrorResult(
                    "Không tìm thấy",
                    "Tài khoản không tồn tại"));
            }

            // Đổi mật khẩu
            account.ChangePassword(request.CurrentPassword, request.NewPassword);
            await _accountRepository.UpdateAsync(account, cancellationToken);
            await _accountRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password changed successfully for account: {AccountId}", accountId);
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
