using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Shared.Common.Constants;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;

namespace Mumii.Discovery.Api.Controllers;

/// <summary>
/// Controller xử lý restaurant discovery
/// </summary>
[ApiController]
[Route(ApiRoutes.Discovery.Base)]
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
    /// Lấy danh sách nhà hàng
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<RestaurantDto>>>> GetRestaurants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var result = await _restaurantRepository.GetPagedAsync(page, pageSize, cancellationToken);
            
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurants");
            return StatusCode(500, ApiResponse<PagedResult<RestaurantDto>>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi lấy danh sách nhà hàng"));
        }
    }

    /// <summary>
    /// Lấy thông tin nhà hàng theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> GetRestaurant(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
            if (restaurant == null)
            {
                return NotFound(ApiResponse<RestaurantDto>.ErrorResult(
                    "Không tìm thấy",
                    "Nhà hàng không tồn tại"));
            }

            var restaurantDto = MapToDto(restaurant);
            return Ok(ApiResponse<RestaurantDto>.SuccessResult(restaurantDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurant {RestaurantId}", id);
            return StatusCode(500, ApiResponse<RestaurantDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi lấy thông tin nhà hàng"));
        }
    }

    /// <summary>
    /// Tìm kiếm nhà hàng
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PagedResult<RestaurantDto>>>> SearchRestaurants(
        [FromQuery] string? q,
        [FromQuery] decimal? lat,
        [FromQuery] decimal? lng,
        [FromQuery] decimal? radiusKm,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] decimal? minRating,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new SearchRestaurantsQuery(
                Query: q,
                Latitude: lat,
                Longitude: lng,
                RadiusKm: radiusKm,
                MinPrice: minPrice,
                MaxPrice: maxPrice,
                MinRating: minRating,
                Page: page < 1 ? 1 : page,
                PageSize: pageSize < 1 || pageSize > 100 ? 20 : pageSize
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching restaurants");
            return StatusCode(500, ApiResponse<PagedResult<RestaurantDto>>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi tìm kiếm nhà hàng"));
        }
    }

    /// <summary>
    /// Tìm nhà hàng gần vị trí
    /// </summary>
    [HttpGet("nearby")]
    public async Task<ActionResult<ApiResponse<List<RestaurantDto>>>> GetNearbyRestaurants(
        [FromQuery] decimal lat,
        [FromQuery] decimal lng,
        [FromQuery] decimal radiusKm = 5.0m,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new NearbyRestaurantsQuery(
                Latitude: lat,
                Longitude: lng,
                RadiusKm: radiusKm,
                Limit: limit > 0 && limit <= 100 ? limit : 50
            );

            var restaurants = await _restaurantRepository.GetNearbyAsync(query, cancellationToken);
            var restaurantDtos = restaurants.Select(MapToDto).ToList();

            return Ok(ApiResponse<List<RestaurantDto>>.SuccessResult(restaurantDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting nearby restaurants");
            return StatusCode(500, ApiResponse<List<RestaurantDto>>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi tìm nhà hàng gần đây"));
        }
    }

    // Xóa hoặc sửa lại endpoint POST cũ, vì giờ Partner sẽ tạo
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateRestaurantByAdmin() 
    {
        // Admin có thể vẫn cần chức năng này, nhưng logic sẽ khác
        return Forbid("Admin should approve restaurants created by partners.");
    }

    /// <summary>
    /// (Admin only) Duyệt một nhà hàng
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> ApproveRestaurant(int id)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id);
        if (restaurant == null) return NotFound();

        try 
        {
            restaurant.Approve();
            await _restaurantRepository.UpdateAsync(restaurant);
            _logger.LogInformation("Admin approved restaurant {RestaurantId}", id);

            // TODO: Gửi thông báo cho Partner
            
            return Ok(ApiResponse.SuccessResult("Duyệt nhà hàng thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResult(ex.Message));
        }
    }

    /// <summary>
    /// (Admin only) Từ chối một nhà hàng
    /// </summary>
    [HttpPost("{id}/decline")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> DeclineRestaurant(int id)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id);
        if (restaurant == null) return NotFound();

        try
        {
            restaurant.Decline();
            await _restaurantRepository.UpdateAsync(restaurant);
            _logger.LogInformation("Admin declined restaurant {RestaurantId}", id);

            // TODO: Gửi thông báo cho Partner
            
            return Ok(ApiResponse.SuccessResult("Từ chối nhà hàng thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResult(ex.Message));
        }
    }

    /// <summary>
    /// Cập nhật nhà hàng (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> UpdateRestaurant(
        int id,
        [FromBody] UpdateRestaurantRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
            if (restaurant == null)
            {
                return NotFound(ApiResponse<RestaurantDto>.ErrorResult(
                    "Không tìm thấy",
                    "Nhà hàng không tồn tại"));
            }

            restaurant.Update(
                name: request.Name,
                address: request.Address,
                latitude: request.Latitude,
                longitude: request.Longitude,
                description: request.Description,
                avgPrice: request.AvgPrice,
                rating: null,
                status: request.Status
            );

            await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);
            await _restaurantRepository.SaveChangesAsync(cancellationToken);

            var restaurantDto = MapToDto(restaurant);
            
            _logger.LogInformation("Restaurant updated: {RestaurantId}", restaurant.Id);
            return Ok(ApiResponse<RestaurantDto>.SuccessResult(restaurantDto, "Cập nhật nhà hàng thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<RestaurantDto>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating restaurant {RestaurantId}", id);
            return StatusCode(500, ApiResponse<RestaurantDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi cập nhật nhà hàng"));
        }
    }

    /// <summary>
    /// Xóa nhà hàng (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> DeleteRestaurant(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _restaurantRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
            {
                return NotFound(ApiResponse.ErrorResult(
                    "Không tìm thấy",
                    "Nhà hàng không tồn tại"));
            }

            await _restaurantRepository.DeleteAsync(id, cancellationToken);
            await _restaurantRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Restaurant deleted: {RestaurantId}", id);
            return Ok(ApiResponse.SuccessResult("Xóa nhà hàng thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting restaurant {RestaurantId}", id);
            return StatusCode(500, ApiResponse.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi xóa nhà hàng"));
        }
    }

    /// <summary>
    /// Map Restaurant entity to DTO
    /// </summary>
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
            Images: new List<RestaurantImageDto>(),
            Reviews: new List<ReviewDto>(),
            FavoriteCount: 0
        );
    }
}
