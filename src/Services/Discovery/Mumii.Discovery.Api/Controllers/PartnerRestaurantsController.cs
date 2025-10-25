using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mumii.Auth.Infrastructure.Services;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Shared.Common.Constants;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Mumii.Discovery.Api.Controllers;

[ApiController]
[Route("api/partner/restaurants")]
[Authorize(Roles = "Partner")]
public class PartnerRestaurantsController : ControllerBase
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMongoIdGenerator _idGenerator;
    private readonly ILogger<PartnerRestaurantsController> _logger;
    private readonly IPhotoService _photoService;
    
    public PartnerRestaurantsController(
        IRestaurantRepository restaurantRepository, 
        IMongoIdGenerator idGenerator, 
        ILogger<PartnerRestaurantsController> logger,
        IPhotoService photoService) 
    { 
        _restaurantRepository = restaurantRepository;
        _idGenerator = idGenerator;
        _logger = logger;
        _photoService = photoService;
    }

    private int GetPartnerId()
    {
        var userIdClaim = User.FindFirstValue("user_id")
                          ?? User.FindFirstValue("nameid")
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy user_id trong token.");

        return int.Parse(userIdClaim);
    }

    [HttpGet("debug")]
    public IActionResult DebugClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(claims);
    }

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

    /// <summary>
    /// (Partner) Xóa một nhà hàng đang ở trạng thái chờ duyệt (Pending)
    /// </summary>
    /// <param name="id">ID của nhà hàng cần xóa</param>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteMyRestaurant(int id)
    {
        try
        {
            var partnerId = GetPartnerId();
            var restaurant = await _restaurantRepository.GetByIdAsync(id);

            // 1. Kiểm tra xem nhà hàng có tồn tại và có thuộc sở hữu của partner này không
            if (restaurant == null || restaurant.PartnerId != partnerId)
            {
                // Không nên báo là "không tìm thấy" để tránh lộ thông tin
                return Forbid();
            }

            // 2. Kiểm tra trạng thái của nhà hàng
            if (restaurant.Status != RestaurantStatus.Pending)
            {
                return BadRequest(ApiResponse.ErrorResult(
                    "Không thể xóa",
                    "Chỉ có thể xóa nhà hàng đang ở trạng thái chờ duyệt (Pending)."));
            }

            // 3. Thực hiện xóa
            await _restaurantRepository.DeleteAsync(id);
            _logger.LogInformation("Partner {PartnerId} deleted pending restaurant {RestaurantId}", partnerId, id);

            return Ok(ApiResponse.SuccessResult("Xóa nhà hàng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting restaurant {RestaurantId} for partner", id);
            return StatusCode(500, ApiResponse.ErrorResult(
                "Lỗi hệ thống", "Đã xảy ra lỗi khi xóa nhà hàng."));
        }
    }

    /// <summary>
    /// (Partner) Tải lên hình ảnh cho nhà hàng của mình
    /// </summary>
    [HttpPost("{restaurantId:int}/images")]
    public async Task<ActionResult<ApiResponse<RestaurantImageDto>>> AddRestaurantImage(
        int restaurantId, IFormFile file)
    {
        try
        {
            var partnerId = GetPartnerId();
            var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

            // 1. Kiểm tra quyền sở hữu
            if (restaurant == null || restaurant.PartnerId != partnerId)
            {
                return Forbid();
            }

            // 2. Kiểm tra file
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse.ErrorResult("Vui lòng chọn một file ảnh."));
            }

            // 3. Tải file lên Cloudinary
            await using var stream = file.OpenReadStream();
            var (url, publicId) = await _photoService.AddPhotoAsync(stream, file.FileName);

            if (url == null || publicId == null)
            {
                return BadRequest(ApiResponse.ErrorResult("Tải ảnh lên thất bại."));
            }

            // 4. Thêm ảnh vào entity và cập nhật DB
            restaurant.AddImage(url, publicId);
            await _restaurantRepository.UpdateAsync(restaurant);

            // 5. Trả về thông tin ảnh vừa tạo
            var newImage = restaurant.Images.Last();
            var imageDto = new RestaurantImageDto(newImage.Id, restaurantId, newImage.ImageUrl, newImage.CreatedAt);
            
            _logger.LogInformation("Image added to restaurant {RestaurantId} by partner {PartnerId}", restaurantId, partnerId);
            return Ok(ApiResponse<RestaurantImageDto>.SuccessResult(imageDto, "Thêm ảnh thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding image to restaurant {RestaurantId}", restaurantId);
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi thêm ảnh."));
        }
    }

    /// <summary>
    /// (Partner) Xóa một hình ảnh khỏi nhà hàng của mình
    /// </summary>
    [HttpDelete("{restaurantId:int}/images/{imageId}")]
    public async Task<ActionResult<ApiResponse>> RemoveRestaurantImage(int restaurantId, string imageId)
    {
        try
        {
            var partnerId = GetPartnerId();
            var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);

            // 1. Kiểm tra quyền sở hữu
            if (restaurant == null || restaurant.PartnerId != partnerId)
            {
                return Forbid();
            }

            // 2. Tìm ảnh cần xóa trong nhà hàng
            var imageToRemove = restaurant.Images.FirstOrDefault(img => img.Id == imageId);
            if (imageToRemove == null)
            {
                return NotFound(ApiResponse.ErrorResult("Không tìm thấy hình ảnh."));
            }

            // 3. Xóa ảnh khỏi Cloudinary
            var deleted = await _photoService.DeletePhotoAsync(imageToRemove.PublicId);
            if (!deleted)
            {
                _logger.LogWarning("Failed to delete photo {PublicId} from Cloudinary for restaurant {RestaurantId}", 
                    imageToRemove.PublicId, restaurantId);
                // Vẫn tiếp tục xóa khỏi DB, nhưng ghi lại log
            }

            // 4. Xóa ảnh khỏi entity và cập nhật DB
            restaurant.RemoveImage(imageId);
            await _restaurantRepository.UpdateAsync(restaurant);
            
            _logger.LogInformation("Image {ImageId} removed from restaurant {RestaurantId}", imageId, restaurantId);
            return Ok(ApiResponse.SuccessResult("Xóa ảnh thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing image from restaurant {RestaurantId}", restaurantId);
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi xóa ảnh."));
        }
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