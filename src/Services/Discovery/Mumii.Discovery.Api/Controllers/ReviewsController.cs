using Microsoft.AspNetCore.Mvc;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Shared.Common.Models;
using Mumii.Shared.Common.DTOs;

namespace Mumii.Discovery.Api.Controllers;

[ApiController]
[Route("api/reviews")] // can be nested under restaurants later if needed
public class ReviewsController : ControllerBase
{
    private readonly IReviewRepository _reviews;

    public ReviewsController(IReviewRepository reviews)
    {
        _reviews = reviews;
    }

    [HttpGet("by-restaurant/{restaurantId}")]
    public async Task<ActionResult<ApiResponse<List<ReviewDto>>>> GetByRestaurant(
        int restaurantId,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var list = await _reviews.GetByRestaurantAsync(restaurantId, skip, limit, cancellationToken);
        var dtos = list.Select(r => new ReviewDto(r.Id, r.UserId, r.RestaurantId, r.Rating, r.Comment ?? string.Empty, r.CreatedAt, null)).ToList();
        return Ok(ApiResponse<List<ReviewDto>>.SuccessResult(dtos));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> Create(
        [FromQuery] int id,
        [FromQuery] int userId,
        [FromQuery] int restaurantId,
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var review = Review.Create(id, userId, restaurantId, request.Rating, request.Comment);
        await _reviews.AddAsync(review, cancellationToken);
        var dto = new ReviewDto(review.Id, review.UserId, review.RestaurantId, review.Rating, review.Comment ?? string.Empty, review.CreatedAt, null);
        return Ok(ApiResponse<ReviewDto>.SuccessResult(dto, "Tạo review thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken = default)
    {
        await _reviews.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResult("Đã xóa review"));
    }
}


