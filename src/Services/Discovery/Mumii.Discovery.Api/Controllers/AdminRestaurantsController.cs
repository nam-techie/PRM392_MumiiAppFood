using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mumii.Discovery.Api.Controllers;

[ApiController]
[Route("api/admin/restaurants")]
[Authorize(Roles = "Admin")] // BẢO VỆ TOÀN BỘ CONTROLLER
public class AdminRestaurantsController : ControllerBase
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ILogger<AdminRestaurantsController> _logger;

    public AdminRestaurantsController(
        IRestaurantRepository restaurantRepository,
        ILogger<AdminRestaurantsController> logger)
    {
        _restaurantRepository = restaurantRepository;
        _logger = logger;
    }

    /// <summary>
    /// (Admin) Lấy danh sách tất cả nhà hàng, có phân trang và lọc theo trạng thái
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<RestaurantDto>>>> GetAllRestaurants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null, // Thêm bộ lọc status
        CancellationToken cancellationToken = default)
    {
        // Bạn cần thêm phương thức GetPagedByStatusAsync vào repository
        var result = await _restaurantRepository.GetPagedByStatusAsync(page, pageSize, status, cancellationToken);
        
        var restaurantDtos = result.Items.Select(MapToDto).ToList();
        var pagedResult = new PagedResult<RestaurantDto>(
            restaurantDtos, result.TotalCount, result.Page, result.PageSize, result.TotalPages
        );

        return Ok(ApiResponse<PagedResult<RestaurantDto>>.SuccessResult(pagedResult));
    }

    /// <summary>
    /// (Admin) Duyệt một nhà hàng
    /// </summary>
    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<ApiResponse>> ApproveRestaurant(int id, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy nhà hàng"));

        try 
        {
            restaurant.Approve();
            await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);
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
    /// (Admin) Từ chối một nhà hàng
    /// </summary>
    [HttpPost("{id:int}/decline")]
    public async Task<ActionResult<ApiResponse>> DeclineRestaurant(int id, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy nhà hàng"));

        try
        {
            restaurant.Decline();
            await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);
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
    /// (Admin) Cập nhật thông tin bất kỳ nhà hàng nào
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> UpdateRestaurant(
        int id, [FromBody] UpdateRestaurantRequest request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null)
        {
            return NotFound(ApiResponse<RestaurantDto>.ErrorResult("Không tìm thấy", "Nhà hàng không tồn tại"));
        }

        try
        {
            restaurant.UpdateByAdmin(
                name: request.Name, address: request.Address, latitude: request.Latitude,
                longitude: request.Longitude, description: request.Description, avgPrice: request.AvgPrice,
                rating: request.Rating, status: request.Status
            );

            await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);
            var restaurantDto = MapToDto(restaurant);
            
            _logger.LogInformation("Admin updated restaurant {RestaurantId}", id);
            return Ok(ApiResponse<RestaurantDto>.SuccessResult(restaurantDto, "Cập nhật nhà hàng thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<RestaurantDto>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
    }

    /// <summary>
    /// (Admin) Xóa một nhà hàng
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteRestaurant(int id, CancellationToken cancellationToken)
    {
        var exists = await _restaurantRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy", "Nhà hàng không tồn tại"));
        }
        await _restaurantRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Admin deleted restaurant {RestaurantId}", id);
        return Ok(ApiResponse.SuccessResult("Xóa nhà hàng thành công"));
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
