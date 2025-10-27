using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Shared.Common.Models;
using Mumii.Shared.Common.DTOs;
using Mumii.Auth.Domain.Interfaces; // Cho IUserRepository, IMongoIdGenerator
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System;

namespace Mumii.Discovery.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IRestaurantRepository _restaurantRepository; // Cần để kiểm tra post tồn tại
    private readonly IUserRepository _userRepository; // Cần để lấy thông tin user
    private readonly IMongoIdGenerator _idGenerator;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(
        IReviewRepository reviewRepository, 
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository,
        IMongoIdGenerator idGenerator, 
        ILogger<ReviewsController> logger)
    {
        _reviewRepository = reviewRepository;
        _restaurantRepository = restaurantRepository;
        _userRepository = userRepository;
        _idGenerator = idGenerator;
        _logger = logger;
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out userId);
    }

    /// <summary>
    /// (Public) Lấy danh sách review của một nhà hàng
    /// </summary>
    [HttpGet("by-restaurant/{restaurantId:int}")]
    public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetReviewsByRestaurant(
        int restaurantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var pagedResult = await _reviewRepository.GetByRestaurantIdAsync(restaurantId, page, pageSize);
        
        if (!pagedResult.Items.Any())
        {
            return Ok(ApiResponse<PagedResult<ReviewDto>>.SuccessResult(new PagedResult<ReviewDto>(new List<ReviewDto>(), 0, page, pageSize, 0)));
        }

        // Lấy thông tin người dùng cho các review
        var userIds = pagedResult.Items.Select(r => r.UserId).Distinct();
        var users = (await _userRepository.GetByIdsAsync(userIds)).ToDictionary(u => u.Id);

        var dtos = pagedResult.Items.Select(r => {
            users.TryGetValue(r.UserId, out var user);
            var userDto = user != null ? new UserDto(user.Id, user.Email, user.Fullname, user.Role, user.IsActive, user.LoginMethod, user.CreatedAt, null) : null;
            return new ReviewDto(r.Id, r.UserId, r.RestaurantId, r.Rating, r.Comment, r.CreatedAt, userDto, r.PartnerReplyComment, r.PartnerReplyAt);
        }).ToList();

        var finalResult = new PagedResult<ReviewDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalPages);
        
        return Ok(ApiResponse<PagedResult<ReviewDto>>.SuccessResult(finalResult));
    }

    /// <summary>
    /// (User) Tạo một review mới cho nhà hàng
    /// </summary>
    [HttpPost("for-restaurant/{restaurantId:int}")]
    [Authorize] // Chỉ user đã đăng nhập mới được review
    public async Task<ActionResult<ApiResponse<ReviewDto>>> CreateReview(int restaurantId, [FromBody] CreateReviewRequest request)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized(ApiResponse.ErrorResult("Token không hợp lệ."));
            
            // TODO: Kiểm tra xem nhà hàng có tồn tại không
            // var restaurantExists = await _restaurantRepository.ExistsAsync(restaurantId);
            // if (!restaurantExists) return NotFound(ApiResponse.ErrorResult("Không tìm thấy nhà hàng."));

            var newId = await _idGenerator.GetNextIdAsync("reviews");
            var review = Review.Create(newId, userId, restaurantId, request.Rating, request.Comment);
            
            var createdReview = await _reviewRepository.AddAsync(review);
            
            var user = await _userRepository.GetByIdAsync(userId);
            var userDto = user != null ? new UserDto(user.Id, user.Email, user.Fullname, user.Role, user.IsActive, user.LoginMethod, user.CreatedAt, null) : null;

            var dto = new ReviewDto(createdReview.Id, createdReview.UserId, createdReview.RestaurantId, createdReview.Rating, createdReview.Comment, createdReview.CreatedAt, userDto, null, null);
            
            _logger.LogInformation("User {UserId} created a review {ReviewId} for restaurant {RestaurantId}", userId, newId, restaurantId);
            return Ok(ApiResponse<ReviewDto>.SuccessResult(dto, "Tạo review thành công"));
        }
        catch(ArgumentException ex) { return BadRequest(ApiResponse.ErrorResult(ex.Message)); }
    }

    /// <summary>
    /// (User) Xóa review của chính mình
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeleteReview(int id)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized(ApiResponse.ErrorResult("Token không hợp lệ."));
            
        var review = await _reviewRepository.GetByIdAsync(id);

        if (review == null || review.UserId != userId)
        {
            // Không cho biết review có tồn tại hay không, chỉ báo không có quyền
            return Forbid();
        }

        await _reviewRepository.DeleteAsync(id);
        _logger.LogInformation("User {UserId} deleted review {ReviewId}", userId, id);
        return Ok(ApiResponse.SuccessResult("Đã xóa review"));
    }
}
