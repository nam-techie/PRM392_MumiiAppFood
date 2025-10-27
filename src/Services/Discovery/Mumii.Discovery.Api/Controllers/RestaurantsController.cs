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

namespace Mumii.Discovery.Api.Controllers;

/// <summary>
/// Controller công khai để khám phá nhà hàng
/// </summary>
[ApiController]
[Route(ApiRoutes.Discovery.Base)] // "api/restaurants"
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ILogger<RestaurantsController> _logger;

    public RestaurantsController(
        IRestaurantRepository restaurantRepository,
        ILogger<RestaurantsController> logger)
    {
        _restaurantRepository = restaurantRepository;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách nhà hàng đã được duyệt
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<RestaurantDto>>>> GetApprovedRestaurants(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // Chỉ hiển thị các nhà hàng đã được "Approved"
        var result = await _restaurantRepository.GetPagedByStatusAsync(page, pageSize, RestaurantStatus.Approved, cancellationToken);
        
        var restaurantDtos = result.Items.Select(MapToDto).ToList();
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
        
        // Chỉ trả về nếu nhà hàng tồn tại VÀ đã được duyệt
        if (restaurant == null || restaurant.Status != RestaurantStatus.Approved)
        {
            return NotFound(ApiResponse<RestaurantDto>.ErrorResult("Không tìm thấy", "Nhà hàng không tồn tại hoặc chưa được duyệt."));
        }

        return Ok(ApiResponse<RestaurantDto>.SuccessResult(MapToDto(restaurant)));
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
        // TODO: Sửa lại logic SearchAsync để nó cũng có thể lọc theo status = "Approved"
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

        var restaurantDtos = result.Items.Select(MapToDto).ToList();
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
        // TODO: Sửa lại logic GetNearbyAsync để nó cũng có thể lọc theo status = "Approved"
        var query = new NearbyRestaurantsQuery(
                Latitude: lat,
                Longitude: lng,
                RadiusKm: radiusKm,
                Limit: limit,
                Status: RestaurantStatus.Approved
            );

        var restaurants = await _restaurantRepository.GetNearbyAsync(query, cancellationToken);
        var restaurantDtos = restaurants.Select(MapToDto).ToList();

        return Ok(ApiResponse<List<RestaurantDto>>.SuccessResult(restaurantDtos));
    }

    private static RestaurantDto MapToDto(Restaurant restaurant)
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
            Reviews: new List<ReviewDto>(), // Giữ nguyên, sẽ làm sau
            FavoriteCount: 0 // Giữ nguyên, sẽ làm sau
        );
    }
}
