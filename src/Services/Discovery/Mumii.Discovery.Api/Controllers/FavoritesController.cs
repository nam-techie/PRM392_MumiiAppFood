using Microsoft.AspNetCore.Mvc;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Shared.Common.Models;
using Mumii.Shared.Common.DTOs;

namespace Mumii.Discovery.Api.Controllers;

[ApiController]
[Route("api/favorites")] // can be nested under users later
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteRepository _favorites;

    public FavoritesController(IFavoriteRepository favorites)
    {
        _favorites = favorites;
    }

    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<FavoriteDto>>>> GetByUser(
        int userId,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var list = await _favorites.GetByUserAsync(userId, skip, limit, cancellationToken);
        var dtos = list.Select(f => new FavoriteDto(f.Id, f.UserId, f.RestaurantId, f.CreatedAt, null!)).ToList();
        return Ok(ApiResponse<List<FavoriteDto>>.SuccessResult(dtos));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FavoriteDto>>> Create(
        [FromQuery] int id,
        [FromQuery] int userId,
        [FromQuery] int restaurantId,
        CancellationToken cancellationToken = default)
    {
        // unique pair check
        if (await _favorites.ExistsAsync(userId, restaurantId, cancellationToken))
            return Ok(ApiResponse<FavoriteDto>.SuccessResult(null!, "Đã yêu thích trước đó"));

        var favorite = Favorite.Create(id, userId, restaurantId);
        await _favorites.AddAsync(favorite, cancellationToken);
        var dto = new FavoriteDto(favorite.Id, favorite.UserId, favorite.RestaurantId, favorite.CreatedAt, null!);
        return Ok(ApiResponse<FavoriteDto>.SuccessResult(dto, "Đã thêm yêu thích"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken = default)
    {
        await _favorites.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResult("Đã bỏ yêu thích"));
    }
}


