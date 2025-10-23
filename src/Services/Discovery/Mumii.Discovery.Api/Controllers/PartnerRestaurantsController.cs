using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mumii.Auth.Infrastructure.Services;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Shared.Common.Constants;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mumii.Discovery.Api.Controllers;

[ApiController]
[Route("api/partner/restaurants")]
[Authorize(Roles = "Partner")]
public class PartnerRestaurantsController : ControllerBase
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMongoIdGenerator _idGenerator;
    private readonly ILogger<PartnerRestaurantsController> _logger;
    
    public PartnerRestaurantsController(
        IRestaurantRepository restaurantRepository, 
        IMongoIdGenerator idGenerator, 
        ILogger<PartnerRestaurantsController> logger) 
    { 
        _restaurantRepository = restaurantRepository;
        _idGenerator = idGenerator;
        _logger = logger;
    }
    
    private int GetPartnerId() => int.Parse(User.FindFirstValue("user_id")!);

    /// <summary>
    /// Partner tạo nhà hàng mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> CreateRestaurant([FromBody] CreateRestaurantRequest request)
    {
        try
        {
            var partnerId = GetPartnerId();
            var newId = await _idGenerator.GetNextIdAsync("restaurants");
            
            var restaurant = Restaurant.Create(
                id: newId,
                partnerId: partnerId,
                name: request.Name,
                address: request.Address,
                latitude: request.Latitude,
                longitude: request.Longitude,
                description: request.Description,
                avgPrice: request.AvgPrice
            );

            await _restaurantRepository.AddAsync(restaurant);

            var restaurantDto = MapToDto(restaurant);
            _logger.LogInformation("Partner {PartnerId} created restaurant {RestaurantId}", partnerId, restaurant.Id);
            
            return CreatedAtAction(
                nameof(GetMyRestaurantById), 
                new { id = restaurant.Id }, 
                ApiResponse<RestaurantDto>.SuccessResult(restaurantDto, "Yêu cầu tạo nhà hàng đã được gửi đi và đang chờ duyệt."));
        }
        catch (ArgumentException ex) {
             return BadRequest(ApiResponse<RestaurantDto>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error creating restaurant for partner {PartnerId}", GetPartnerId());
            return StatusCode(500, ApiResponse<RestaurantDto>.ErrorResult("Lỗi hệ thống khi tạo nhà hàng."));
        }
    }

    /// <summary>
    /// Partner lấy danh sách nhà hàng của mình
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RestaurantDto>>>> GetMyRestaurants()
    {
        var partnerId = GetPartnerId();
        var restaurants = await _restaurantRepository.GetByPartnerIdAsync(partnerId); 
        var dtos = restaurants.Select(MapToDto).ToList();
        return Ok(ApiResponse<List<RestaurantDto>>.SuccessResult(dtos));
    }

    /// <summary>
    /// Partner lấy một nhà hàng cụ thể của mình theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> GetMyRestaurantById(int id)
    {
        var partnerId = GetPartnerId();
        var restaurant = await _restaurantRepository.GetByIdAsync(id);

        if (restaurant == null || restaurant.PartnerId != partnerId)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy nhà hàng."));
        }
        
        return Ok(ApiResponse<RestaurantDto>.SuccessResult(MapToDto(restaurant)));
    }


    /// <summary>
    /// Partner cập nhật nhà hàng của mình
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> UpdateMyRestaurant(int id, [FromBody] UpdateRestaurantRequest request)
    {
        var partnerId = GetPartnerId();
        var restaurant = await _restaurantRepository.GetByIdAsync(id);

        if (restaurant == null || restaurant.PartnerId != partnerId)
        {
            return Forbid(); // Hoặc NotFound
        }
        
        restaurant.UpdateByPartner(request.Name, request.Address, request.Description, request.AvgPrice); 
        await _restaurantRepository.UpdateAsync(restaurant);
        
        var restaurantDto = MapToDto(restaurant);
        return Ok(ApiResponse<RestaurantDto>.SuccessResult(restaurantDto));
    }

    private static RestaurantDto MapToDto(Restaurant r) {
        return new RestaurantDto(
            r.Id,
            r.PartnerId,
            r.Name,
            r.Address,
            r.Longitude,
            r.Latitude,
            r.Description,
            r.AvgPrice,
            r.Rating,
            r.Status,
            r.CreatedAt,
            new List<RestaurantImageDto>(),
            new List<ReviewDto>(),
            0
        );
    }
}