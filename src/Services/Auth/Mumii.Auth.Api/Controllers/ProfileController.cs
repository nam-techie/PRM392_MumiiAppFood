using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Services;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;

namespace Mumii.Auth.Api.Controllers;

/// <summary>
/// Controller xử lý quản lý profile người dùng
/// </summary>
[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ProfileController> _logger;
    private readonly IProfileRepository _profileRepository; 
    private readonly IMongoIdGenerator _idGenerator;

    public ProfileController(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IMongoIdGenerator idGenerator,
        ILogger<ProfileController> logger)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _idGenerator = idGenerator;
        _logger = logger;
    }

    /// <summary>
    /// Lấy thông tin profile hiện tại
    /// </summary>
    [HttpGet("me")]
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
                    "Không xác thực", "Token không hợp lệ"));
            }

            // BƯỚC 1: Lấy thông tin User cơ bản
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResult(
                    "Không tìm thấy", "Tài khoản không tồn tại"));
            }

            // BƯỚC 2: Lấy thông tin Profile riêng biệt
            var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);

            // BƯỚC 3: Map cả hai sang DTO
            ProfileDto? profileDto = null;
            if (profile != null)
            {
                profileDto = new ProfileDto(
                    profile.Id,
                    profile.UserId,
                    profile.Gender,
                    profile.Avatar,
                    profile.PhoneNumber,
                    profile.Address
                );
            }

            var userDto = new UserDto(
                user.Id,
                user.Email,
                user.Fullname,
                user.Role,
                user.IsActive,
                user.LoginMethod,
                user.CreatedAt,
                profileDto // Gán profileDto đã được load vào đây
            );

            return Ok(ApiResponse<UserDto>.SuccessResult(userDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResult(
                "Lỗi hệ thống", "Đã xảy ra lỗi khi lấy thông tin profile"));
        }
    }

    /// <summary>
    /// Tạo hoặc Cập nhật thông tin profile
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst("user_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(ApiResponse<ProfileDto>.ErrorResult(
                    "Không xác thực", "Token không hợp lệ"));
            }

            // Lấy thông tin User
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound(ApiResponse<ProfileDto>.ErrorResult(
                    "Không tìm thấy", "Tài khoản không tồn tại"));
            }

            // Cập nhật họ tên của User (nếu có trong request)
            if (!string.IsNullOrWhiteSpace(request.Fullname) && user.Fullname != request.Fullname)
            {
                user.UpdateBasicInfo(request.Fullname);
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            // Tìm profile của user
            var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);

            // >>> LOGIC MỚI: Chuẩn hóa dữ liệu đầu vào <<<
            var gender = request.Gender == "Chưa cập nhật" ? null : request.Gender;
            var phoneNumber = request.PhoneNumber == "Chưa cập nhật" ? null : request.PhoneNumber;
            var address = request.Address == "Chưa cập nhật" ? null : request.Address;

            if (profile == null)
            {
                // **TRƯỜNG HỢP 1: PROFILE CHƯA TỒN TẠI -> TẠO MỚI**
                var newProfileId = await _idGenerator.GetNextIdAsync("profiles", cancellationToken);

                profile = Profile.Create(
                    newProfileId,
                    userId,
                    gender, // Dùng biến đã chuẩn hóa
                    null,
                    phoneNumber, // Dùng biến đã chuẩn hóa
                    address // Dùng biến đã chuẩn hóa
                );

                await _profileRepository.AddAsync(profile, cancellationToken);
                _logger.LogInformation("Profile created for user {UserId}", userId);
            }
            else
            {
                // **TRƯỜ-NG HỢP 2: PROFILE ĐÃ TỒN TẠI -> CẬP NHẬT**
                profile.Update(
                    gender, // Dùng biến đã chuẩn hóa
                    null,
                    phoneNumber, // Dùng biến đã chuẩn hóa
                    address // Dùng biến đã chuẩn hóa
                );

                await _profileRepository.UpdateAsync(profile, cancellationToken);
                _logger.LogInformation("Profile updated for user {UserId}", userId);
            }

            // Map kết quả sang DTO để trả về
            var profileDto = new ProfileDto(
                profile.Id,
                profile.UserId,
                profile.Gender,
                profile.Avatar,
                profile.PhoneNumber,
                profile.Address
            );

            return Ok(ApiResponse<ProfileDto>.SuccessResult(profileDto, "Cập nhật hồ sơ thành công"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid profile data provided: {Message}", ex.Message);
            return BadRequest(ApiResponse<ProfileDto>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, ApiResponse<ProfileDto>.ErrorResult(
                "Lỗi hệ thống", "Đã xảy ra lỗi khi cập nhật profile"));
        }
    }

    /// <summary>
    /// Tải lên/Cập nhật avatar
    /// </summary>
    [HttpPost("avatar")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> UploadAvatar(
        IFormFile avatar,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst("user_id")?.Value ??
                           User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(
                    "Không xác thực",
                    "Token không hợp lệ"));
            }

            if (avatar == null || avatar.Length == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(
                    "Dữ liệu không hợp lệ",
                    "Vui lòng chọn file ảnh"));
            }

            // TODO: Implement file upload logic (S3, Cloudinary, etc.)
            // For now, return not implemented
            return Ok(ApiResponse<object>.ErrorResult(
                "Chưa triển khai",
                "Upload avatar chưa được triển khai"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar");
            return StatusCode(500, ApiResponse<object>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi tải lên avatar"));
        }
    }
}
