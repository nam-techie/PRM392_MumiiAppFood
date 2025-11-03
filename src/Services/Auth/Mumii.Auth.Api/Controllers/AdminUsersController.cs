using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Services;
using Mumii.Shared.Common.Constants;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Mumii.Auth.Api.Controllers;

/// <summary>
/// Controller cho Admin quản lý người dùng
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")] // <<< CHỈ ADMIN MỚI CÓ QUYỀN TRUY CẬP
public class AdminUsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IProfileRepository _profileRepository; // <<< THÊM DÒNG NÀY
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        IUserRepository userRepository,
        IProfileRepository profileRepository, // <<< THÊM THAM SỐ NÀY
        ILogger<AdminUsersController> logger)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository; // <<< GÁN GIÁ TRỊ
        _logger = logger;
    }

    /// <summary>
    /// (Admin) Lấy danh sách tất cả người dùng có phân trang
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var pagedUsers = await _userRepository.GetPagedAsync(page, pageSize);

        // >>> LOGIC MỚI: TẢI PROFILE CHO TỪNG USER <<<
        var userDtos = new List<UserDto>();
        foreach (var user in pagedUsers.Items)
        {
            // Tải profile tương ứng
            var profile = await _profileRepository.GetByUserIdAsync(user.Id);
            // Map và thêm vào danh sách
            userDtos.Add(MapToDto(user, profile));
        }
        // >>> ------------------------------------ <<<

        var result = new PagedResult<UserDto>(
            userDtos, // <-- Dùng danh sách đã có profile
            pagedUsers.TotalCount,
            pagedUsers.Page,
            pagedUsers.PageSize,
            pagedUsers.TotalPages
        );

        return Ok(ApiResponse<PagedResult<UserDto>>.SuccessResult(result));
    }

    /// <summary>
    /// (Admin) Lấy thông tin chi tiết của một người dùng
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy người dùng."));
        }

        // >>> LOGIC MỚI: TẢI PROFILE CỦA USER <<<
        var profile = await _profileRepository.GetByUserIdAsync(id);

        // Map cả user và profile (có thể là null) sang DTO
        return Ok(ApiResponse<UserDto>.SuccessResult(MapToDto(user, profile)));
    }

    /// <summary>
    /// (Admin) Cập nhật thông tin của một người dùng
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(int id, [FromBody] AdminUpdateUserRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResult("Không tìm thấy người dùng."));
            }

            // Sử dụng các phương thức đã có trong User entity
            user.UpdateBasicInfo(request.Fullname);
            user.SetRole(request.Role);
            user.SetActive(request.IsActive);

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Admin updated user {UserId}", id);

            return Ok(ApiResponse<UserDto>.SuccessResult(MapToDto(user, null), "Cập nhật người dùng thành công."));
        }
        catch (System.ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResult(ex.Message));
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi cập nhật người dùng."));
        }
    }

    /// <summary>
    /// (Admin) Vô hiệu hóa (khóa) một tài khoản người dùng
    /// </summary>
    [HttpPost("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponse>> DeactivateUser(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy người dùng."));
        }

        user.SetActive(false);
        await _userRepository.UpdateAsync(user);

        _logger.LogWarning("Admin deactivated user {UserId}", id);
        return Ok(ApiResponse.SuccessResult("Vô hiệu hóa tài khoản thành công."));
    }

    /// <summary>
    /// (Admin) Kích hoạt lại một tài khoản người dùng
    /// </summary>
    [HttpPost("{id:int}/activate")]
    public async Task<ActionResult<ApiResponse>> ActivateUser(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy người dùng."));
        }

        user.SetActive(true);
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Admin activated user {UserId}", id);
        return Ok(ApiResponse.SuccessResult("Kích hoạt tài khoản thành công."));
    }

    // Helper method để map sang DTO
    private static UserDto MapToDto(User user, Profile? profile)
    {
        ProfileDto? profileDto = null;
        if (profile != null)
        {
            profileDto = new ProfileDto(profile.Id, profile.UserId, profile.Gender, profile.Avatar, profile.PhoneNumber, profile.Address);
        }

        return new UserDto(
            user.Id,
            user.Email,
            user.Fullname,
            user.Role,
            user.IsActive,
            user.LoginMethod,
            user.CreatedAt,
            profileDto
        );
    }
}