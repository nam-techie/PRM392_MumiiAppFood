using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Shared.Common.Constants;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mumii.Auth.Domain.Interfaces;

namespace Mumii.Discovery.Api.Controllers;

/// <summary>
/// Controller công khai để khám phá nhà hàng
/// </summary>
[ApiController]
[Route(ApiRoutes.Discovery.Base)] // "api/restaurants"
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IReviewRepository _reviewRepository; // << FIX: Inject Review Repository
    private readonly IUserRepository _userRepository;     // << FIX: Inject User Repository
    private readonly ILogger<RestaurantsController> _logger;

    public RestaurantsController(
        IRestaurantRepository restaurantRepository,
        IReviewRepository reviewRepository, 
        IUserRepository userRepository,
        ILogger<RestaurantsController> logger)
    {
        _restaurantRepository = restaurantRepository;
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách nhà hàng đã được duyệt
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<RestaurantDto>>>> GetApprovedRestaurants(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _restaurantRepository.GetPagedByStatusAsync(page, pageSize, RestaurantStatus.Approved, cancellationToken);
        
        var restaurantDtos = result.Items.Select(r => MapToDto(r)).ToList(); // FIX: Bỏ static
        var pagedResult = new PagedResult<RestaurantDto>(
            restaurantDtos, result.TotalCount, result.Page, result.PageSize, result.TotalPages
        );
        return Ok(ApiResponse<PagedResult<RestaurantDto>>.SuccessResult(pagedResult));
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một nhà hàng đã được duyệt
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> GetRestaurant(int id, CancellationToken cancellationToken = default)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        
        if (restaurant == null || restaurant.Status != RestaurantStatus.Approved)
        {
            return NotFound(ApiResponse<RestaurantDto>.ErrorResult("Không tìm thấy", "Nhà hàng không tồn tại hoặc chưa được duyệt."));
        }

        // === FIX START: Lấy reviews cho nhà hàng ===
        // Lấy 5 review đầu tiên
        var reviewsResult = await _reviewRepository.GetByRestaurantIdAsync(id, 1, 5);
        var reviewDtos = new List<ReviewDto>();

        if (reviewsResult.Items.Any())
        {
            var userIds = reviewsResult.Items.Select(r => r.UserId).Distinct();
            var users = (await _userRepository.GetByIdsAsync(userIds)).ToDictionary(u => u.Id);

            reviewDtos = reviewsResult.Items.Select(r => {
                users.TryGetValue(r.UserId, out var user);
                var userDto = user != null ? new UserDto(user.Id, user.Email, user.Fullname, user.Role, user.IsActive, user.LoginMethod, user.CreatedAt, null) : null;
                return new ReviewDto(r.Id, r.UserId, r.RestaurantId, r.Rating, r.Comment, r.CreatedAt, userDto, r.PartnerReplyComment, r.PartnerReplyAt);
            }).ToList();
        }
        // === FIX END ===

        return Ok(ApiResponse<RestaurantDto>.SuccessResult(MapToDto(restaurant, reviewDtos)));
    }

    /// <summary>
    /// Tìm kiếm trong các nhà hàng đã được duyệt
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PagedResult<RestaurantDto>>>> SearchRestaurants(
        [FromQuery] string? q,
        [FromQuery] double? lat,
        [FromQuery] double? lng,
        [FromQuery] double? radiusKm,
        [FromQuery] double? minPrice,
        [FromQuery] double? maxPrice,
        [FromQuery] float? minRating,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchRestaurantsQuery(
                Query: q,
                Latitude: lat,
                Longitude: lng,
                RadiusKm: radiusKm,
                MinPrice: minPrice,
                MaxPrice: maxPrice,
                MinRating: minRating,
                Page: page,
                PageSize: pageSize,
                Status: RestaurantStatus.Approved
            );

        var result = await _restaurantRepository.SearchAsync(query, cancellationToken);

        var restaurantDtos = result.Items.Select(r => MapToDto(r)).ToList(); // FIX: Bỏ static
        var pagedResult = new PagedResult<RestaurantDto>(
            restaurantDtos,
            result.TotalCount,
            result.Page,
            result.PageSize,
            result.TotalPages
        );

        return Ok(ApiResponse<PagedResult<RestaurantDto>>.SuccessResult(pagedResult));
    }

    /// <summary>
    /// Tìm nhà hàng gần vị trí
    /// </summary>
    [HttpGet("nearby")]
    public async Task<ActionResult<ApiResponse<List<RestaurantDto>>>> GetNearbyRestaurants(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radiusKm = 5.0,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new NearbyRestaurantsQuery(
                Latitude: lat,
                Longitude: lng,
                RadiusKm: radiusKm,
                Limit: limit,
                Status: RestaurantStatus.Approved
            );

        var restaurants = await _restaurantRepository.GetNearbyAsync(query, cancellationToken);
        var restaurantDtos = restaurants.Select(r => MapToDto(r)).ToList(); // FIX: Bỏ static

        return Ok(ApiResponse<List<RestaurantDto>>.SuccessResult(restaurantDtos));
    }

    // FIX: Bỏ static để có thể truy cập các repository
    private RestaurantDto MapToDto(Restaurant restaurant, List<ReviewDto>? reviews = null)
    {
        return new RestaurantDto(
            Id: restaurant.Id,
            PartnerId: restaurant.PartnerId,
            Name: restaurant.Name,
            Address: restaurant.Address,
            Longitude: restaurant.Longitude,
            Latitude: restaurant.Latitude,
            Description: restaurant.Description,
            AvgPrice: restaurant.AvgPrice,
            Rating: restaurant.Rating,
            Status: restaurant.Status,
            CreatedAt: restaurant.CreatedAt,
            Images: restaurant.Images?.Select(img => new RestaurantImageDto(
                Id: img.Id,
                RestaurantId: restaurant.Id,
                ImageUrl: img.ImageUrl,
                CreatedAt: img.CreatedAt
            )).ToList() ?? new List<RestaurantImageDto>(),
            Reviews: reviews ?? new List<ReviewDto>(), // << FIX: Sử dụng review đã fetch
            FavoriteCount: 0 // Giữ nguyên, sẽ làm sau
        );
    }
}
