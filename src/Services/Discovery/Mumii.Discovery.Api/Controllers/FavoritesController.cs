using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Services;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Mumii.Discovery.Domain.Entities;

namespace Mumii.Discovery.Api.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize] // Yêu cầu đăng nhập cho tất cả các API trong đây
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IRestaurantRepository _restaurantRepository; // Cần để lấy thông tin nhà hàng
    private readonly IMongoIdGenerator _idGenerator; // Cần để tạo ID
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(
        IFavoriteRepository favoriteRepository,
        IRestaurantRepository restaurantRepository,
        IMongoIdGenerator idGenerator,
        ILogger<FavoritesController> logger)
    {
        _favoriteRepository = favoriteRepository;
        _restaurantRepository = restaurantRepository;
        _idGenerator = idGenerator;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        // === FIX START ===
        // Thay đổi để lấy claim "user_id" một cách tường minh, an toàn hơn.
        var userIdStr = User.FindFirstValue("user_id"); 
        // === FIX END ===

        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("Không thể xác thực người dùng từ token.");
        }
        return userId;
    }

    /// <summary>
    /// (User) Lấy danh sách nhà hàng yêu thích của chính mình
    /// </summary>
    [HttpGet("my-favorites")]
    public async Task<ActionResult<ApiResponse<List<FavoriteDto>>>> GetMyFavorites()
    {
        try
        {
            var userId = GetCurrentUserId();
            var favorites = await _favoriteRepository.GetByUserAsync(userId);

            if (!favorites.Any())
            {
                return Ok(ApiResponse<List<FavoriteDto>>.SuccessResult(new List<FavoriteDto>()));
            }

            var restaurantIds = favorites.Select(f => f.RestaurantId);
            
            var restaurantList = await _restaurantRepository.GetByIdsAsync(restaurantIds);
            var restaurants = restaurantList?.ToDictionary(r => r.Id) ?? new Dictionary<int, Restaurant>();

            var dtos = favorites.Select(f => 
            {
                restaurants.TryGetValue(f.RestaurantId, out var restaurant);
                var restaurantDto = restaurant != null ? new Mumii.Shared.Common.DTOs.RestaurantDto(restaurant.Id, restaurant.PartnerId, restaurant.Name, restaurant.Address, restaurant.Longitude, restaurant.Latitude, restaurant.Description, restaurant.AvgPrice, restaurant.Rating, restaurant.Status, restaurant.CreatedAt, new List<Mumii.Shared.Common.DTOs.RestaurantImageDto>(), new List<Mumii.Shared.Common.DTOs.ReviewDto>(), 0) : null;
                return new FavoriteDto(f.Id, f.UserId, f.RestaurantId, f.CreatedAt, restaurantDto);
            }).ToList();

            return Ok(ApiResponse<List<FavoriteDto>>.SuccessResult(dtos));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access in GetMyFavorites.");
            return Unauthorized(ApiResponse.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorites for user.");
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi lấy danh sách yêu thích."));
        }
    }

    /// <summary>
    /// (User) Thêm một nhà hàng vào danh sách yêu thích
    /// </summary>
    [HttpPost("{restaurantId:int}")]
    public async Task<ActionResult<ApiResponse<FavoriteDto>>> AddFavorite(int restaurantId)
    {
        try
        {
            var userId = GetCurrentUserId();

            var restaurantExists = await _restaurantRepository.ExistsAsync(restaurantId);
            if (!restaurantExists)
            {
                return NotFound(ApiResponse.ErrorResult("Không tìm thấy nhà hàng."));
            }

            if (await _favoriteRepository.ExistsAsync(userId, restaurantId))
            {
                return BadRequest(ApiResponse.ErrorResult("Bạn đã yêu thích nhà hàng này rồi."));
            }

            var newId = await _idGenerator.GetNextIdAsync("favorites");
            var favorite = Favorite.Create(newId, userId, restaurantId);
            
            await _favoriteRepository.AddAsync(favorite);
            _logger.LogInformation("User {UserId} favorited restaurant {RestaurantId}", userId, restaurantId);

            var dto = new FavoriteDto(favorite.Id, favorite.UserId, favorite.RestaurantId, favorite.CreatedAt, null);
            return Ok(ApiResponse<FavoriteDto>.SuccessResult(dto, "Thêm vào yêu thích thành công."));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access in AddFavorite.");
            return Unauthorized(ApiResponse.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error adding favorite for user.");
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi thêm yêu thích."));
        }
    }

    /// <summary>
    /// (User) Xóa một nhà hàng khỏi danh sách yêu thích
    /// </summary>
    [HttpDelete("{restaurantId:int}")]
    public async Task<ActionResult<ApiResponse>> RemoveFavorite(int restaurantId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var favorite = await _favoriteRepository.GetByUserAndRestaurantAsync(userId, restaurantId);

            if (favorite == null)
            {
                return NotFound(ApiResponse.ErrorResult("Nhà hàng này không có trong danh sách yêu thích của bạn."));
            }

            await _favoriteRepository.DeleteAsync(favorite.Id);
            _logger.LogInformation("User {UserId} unfavorited restaurant {RestaurantId}", userId, restaurantId);

            return Ok(ApiResponse.SuccessResult("Đã xóa khỏi danh sách yêu thích."));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access in RemoveFavorite.");
            return Unauthorized(ApiResponse.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error removing favorite for user.");
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi xóa yêu thích."));
        }
    }
}

public record FavoriteDto(int Id, int UserId, int RestaurantId, DateTime CreatedAt, Mumii.Shared.Common.DTOs.RestaurantDto? Restaurant);
