using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Mumii.Auth.Domain.Interfaces; // Thêm

namespace Mumii.Discovery.Api.Controllers;

[ApiController]
[Route("api/partner")]
[Authorize(Roles = "Partner")]
public class PartnerReviewsController : ControllerBase
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUserRepository _userRepository; // Thêm
    private readonly ILogger<PartnerReviewsController> _logger;

    public PartnerReviewsController(
        IReviewRepository reviewRepository,
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository, // Thêm
        ILogger<PartnerReviewsController> logger)
    {
        _reviewRepository = reviewRepository;
        _restaurantRepository = restaurantRepository;
        _userRepository = userRepository; // Thêm
        _logger = logger;
    }

    private int GetPartnerId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userIdStr))
        {
            userIdStr = User.FindFirstValue("user_id");
        }

        if (string.IsNullOrEmpty(userIdStr))
        {
            throw new InvalidOperationException("User ID claim not found in token.");
        }

        return int.Parse(userIdStr);
    }

    /// <summary>
    /// Lấy danh sách đánh giá cho một nhà hàng của partner
    /// </summary>
    [HttpGet("restaurants/{restaurantId:int}/reviews")]
    public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetReviewsForRestaurant(
        int restaurantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var partnerId = GetPartnerId();
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

        if (restaurant == null || restaurant.PartnerId != partnerId)
        {
            return Forbid(); // Không có quyền xem review của nhà hàng này
        }

        var pagedReviews = await _reviewRepository.GetByRestaurantIdAsync(restaurantId, page, pageSize);
        
        if (!pagedReviews.Items.Any())
        {
            return Ok(ApiResponse<PagedResult<ReviewDto>>.SuccessResult(new PagedResult<ReviewDto>(new List<ReviewDto>(), 0, page, pageSize, 0)));
        }

        // Lấy thông tin người dùng cho các review
        var userIds = pagedReviews.Items.Select(r => r.UserId).Distinct();
        var users = (await _userRepository.GetByIdsAsync(userIds)).ToDictionary(u => u.Id);

        var reviewDtos = pagedReviews.Items.Select(r => {
            users.TryGetValue(r.UserId, out var user);
            var userDto = user != null ? new UserDto(user.Id, user.Email, user.Fullname, user.Role, user.IsActive, user.LoginMethod, user.CreatedAt, null) : null;
            return new ReviewDto(r.Id, r.UserId, r.RestaurantId, r.Rating, r.Comment, r.CreatedAt, userDto, r.PartnerReplyComment, r.PartnerReplyAt);
        }).ToList();
        
        var result = new PagedResult<ReviewDto>(reviewDtos, pagedReviews.TotalCount, pagedReviews.Page, pagedReviews.PageSize, pagedReviews.TotalPages);

        return Ok(ApiResponse<PagedResult<ReviewDto>>.SuccessResult(result));
    }

    /// <summary>
    /// Partner trả lời một đánh giá
    /// </summary>
    [HttpPost("reviews/{reviewId:int}/reply")]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> ReplyToReview(int reviewId, [FromBody] ReplyToReviewRequest request)
    {
        var partnerId = GetPartnerId();
        var review = await _reviewRepository.GetByIdAsync(reviewId);

        if (review == null)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy đánh giá."));
        }

        // Kiểm tra quyền sở hữu
        var restaurant = await _restaurantRepository.GetByIdAsync(review.RestaurantId);
        if (restaurant == null || restaurant.PartnerId != partnerId)
        {
            return Forbid();
        }

        try
        {
            review.AddOrUpdateReply(request.Comment);
            await _reviewRepository.UpdateAsync(review);

            var reviewDto = new ReviewDto(review.Id, review.UserId, review.RestaurantId, review.Rating, review.Comment, review.CreatedAt, null, review.PartnerReplyComment, review.PartnerReplyAt);
            
            _logger.LogInformation("Partner {PartnerId} replied to review {ReviewId}", partnerId, reviewId);
            return Ok(ApiResponse<ReviewDto>.SuccessResult(reviewDto, "Trả lời đánh giá thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResult(ex.Message));
        }
    }
}

public record ReplyToReviewRequest(string Comment);
