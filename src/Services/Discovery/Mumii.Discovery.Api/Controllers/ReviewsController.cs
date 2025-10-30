using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Shared.Common.Models;
using Mumii.Shared.Common.DTOs;
using Mumii.Auth.Domain.Interfaces; // Cho IUserRepository, IMongoIdGenerator
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
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUserRepository _userRepository;
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
        var userIdStr = User.FindFirstValue("user_id");
        return !string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out userId);
    }

    private async Task RecalculateRestaurantRating(int restaurantId)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
        if (restaurant == null)
        {
            _logger.LogWarning("Restaurant {RestaurantId} not found when recalculating rating.", restaurantId);
            return;
        }

        var allReviews = await _reviewRepository.GetByRestaurantIdAsync(restaurantId, 1, int.MaxValue); // Lấy tất cả reviews
        if (allReviews.Items.Any())
        {
            var averageRating = allReviews.Items.Average(r => r.Rating);
            restaurant.UpdateRating((float)averageRating);
        }
        else
        {
            restaurant.UpdateRating(0.0f); // Không có review nào, đặt rating về 0
        }
        await _restaurantRepository.UpdateAsync(restaurant);
        _logger.LogInformation("Recalculated rating for restaurant {RestaurantId} to {Rating}", restaurantId, restaurant.Rating);
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
            
            var restaurantExists = await _restaurantRepository.ExistsAsync(restaurantId);
            if (!restaurantExists) return NotFound(ApiResponse.ErrorResult("Không tìm thấy nhà hàng."));

            // Kiểm tra xem người dùng đã review nhà hàng này chưa
            var hasReviewed = await _reviewRepository.HasUserReviewedRestaurantAsync(userId, restaurantId);
            if (hasReviewed)
            {
                return BadRequest(ApiResponse.ErrorResult("Bạn đã đánh giá nhà hàng này rồi."));
            }

            var newId = await _idGenerator.GetNextIdAsync("reviews");
            var review = Review.Create(newId, userId, restaurantId, request.Rating, request.Comment);
            
            var createdReview = await _reviewRepository.AddAsync(review);
            
            await RecalculateRestaurantRating(restaurantId);

            var user = await _userRepository.GetByIdAsync(userId);
            var userDto = user != null ? new UserDto(user.Id, user.Email, user.Fullname, user.Role, user.IsActive, user.LoginMethod, user.CreatedAt, null) : null;

            var dto = new ReviewDto(createdReview.Id, createdReview.UserId, createdReview.RestaurantId, createdReview.Rating, createdReview.Comment, createdReview.CreatedAt, userDto, null, null);
            
            _logger.LogInformation("User {UserId} created a review {ReviewId} for restaurant {RestaurantId}", userId, newId, restaurantId);
            return Ok(ApiResponse<ReviewDto>.SuccessResult(dto, "Tạo review thành công"));
        }
        catch(ArgumentException ex) { return BadRequest(ApiResponse.ErrorResult(ex.Message)); }
    }

    /// <summary>
    /// (User) Cập nhật review của chính mình
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> UpdateReview(int id, [FromBody] UpdateReviewRequest request)
    {
        int userId = 0; // Khai báo userId ở đây
        try
        {
            if (!TryGetCurrentUserId(out userId))
                return Unauthorized(ApiResponse.ErrorResult("Token không hợp lệ."));

            var review = await _reviewRepository.GetByIdAsync(id);

            if (review == null)
            {
                return NotFound(ApiResponse.ErrorResult("Không tìm thấy review."));
            }

            if (review.UserId != userId)
            {
                return Forbid(); // Không có quyền cập nhật review của người khác
            }

            review.Update(request.Rating, request.Comment); // Sửa từ UpdateReview thành Update
            await _reviewRepository.UpdateAsync(review);

            await RecalculateRestaurantRating(review.RestaurantId);

            var user = await _userRepository.GetByIdAsync(userId);
            var userDto = user != null ? new UserDto(user.Id, user.Email, user.Fullname, user.Role, user.IsActive, user.LoginMethod, user.CreatedAt, null) : null;

            var dto = new ReviewDto(review.Id, review.UserId, review.RestaurantId, review.Rating, review.Comment, review.CreatedAt, userDto, review.PartnerReplyComment, review.PartnerReplyAt);

            _logger.LogInformation("User {UserId} updated review {ReviewId}", userId, id);
            return Ok(ApiResponse<ReviewDto>.SuccessResult(dto, "Cập nhật review thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId} by user {UserId}", id, userId);
            return StatusCode(500, ApiResponse.ErrorResult("Đã xảy ra lỗi nội bộ khi cập nhật review."));
        }
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
            return Forbid();
        }

        var restaurantId = review.RestaurantId; // Lưu lại restaurantId trước khi xóa review
        await _reviewRepository.DeleteAsync(id);
        
        await RecalculateRestaurantRating(restaurantId);

        _logger.LogInformation("User {UserId} deleted review {ReviewId}", userId, id);
        return Ok(ApiResponse.SuccessResult("Đã xóa review"));
    }
}
