using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
// Removed conflicting domain interface usings to avoid ambiguity with Social.Domain.Interfaces
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;
using Mumii.Auth.Infrastructure.Services;
using AuthPhotoService = Mumii.Auth.Domain.Interfaces.IPhotoService;


namespace Mumii.Social.Api.Controllers;

[ApiController]
[Route("api/partner/posts")]
[Authorize(Roles = "Partner")]
public class PartnerPostsController : ControllerBase
{
    private readonly IPostRepository _postRepository;
    private readonly IMongoIdGenerator _idGenerator;
    private readonly ILogger<PartnerPostsController> _logger;
    private readonly AuthPhotoService _photoService; // <-- Đảm bảo đã inject
    // Inject các repo cần thiết để map DTO
    private readonly IUserRepository _userRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMoodRepository _moodRepository;

    public PartnerPostsController(
        IPostRepository postRepository,
        IMongoIdGenerator idGenerator,
        ILogger<PartnerPostsController> logger,
        IUserRepository userRepository,
        IRestaurantRepository restaurantRepository,
        IMoodRepository moodRepository,
        AuthPhotoService photoService) // <-- Thêm IPhotoService vào constructor
    {
        _postRepository = postRepository;
        _idGenerator = idGenerator;
        _logger = logger;
        _userRepository = userRepository;
        _restaurantRepository = restaurantRepository;
        _moodRepository = moodRepository;
        _photoService = photoService;
    }

    private int GetCurrentPartnerId()
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
    /// Partner lấy danh sách các bài đăng của chính mình
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PostDto>>>> GetMyPosts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var partnerId = GetCurrentPartnerId();
        var pagedPosts = await _postRepository.GetPagedAsync(page, pageSize, partnerId);
        
        var dtos = await MapPostsToDtosAsync(pagedPosts.Items);
        
        var result = new PagedResult<PostDto>(dtos.ToList(), pagedPosts.TotalCount, pagedPosts.Page, pagedPosts.PageSize, pagedPosts.TotalPages);
        return Ok(ApiResponse<PagedResult<PostDto>>.SuccessResult(result));
    }

    /// <summary>
    /// Partner tạo một bài đăng mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PostDto>>> CreatePost([FromBody] CreatePostRequest request)
    {
        try
        {
            var partnerId = GetCurrentPartnerId();
            var newId = await _idGenerator.GetNextIdAsync("posts");
            
            var post = Post.Create(newId, partnerId, request.Title, request.Content, request.ImageUrl, request.RestaurantId);
            
            // TODO: Xử lý gán Moods từ request.MoodIds
            
            await _postRepository.AddAsync(post);
            _logger.LogInformation("Partner {PartnerId} created post {PostId}", partnerId, post.Id);

            var dtos = await MapPostsToDtosAsync(new List<Post> { post });
            return Ok(ApiResponse<PostDto>.SuccessResult(dtos.First(), "Tạo bài đăng thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResult(ex.Message));
        }
    }

    /// <summary>
    /// Partner cập nhật bài đăng của mình
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> UpdatePost(int id, [FromBody] UpdatePostRequest request)
    {
        var partnerId = GetCurrentPartnerId();
        var post = await _postRepository.GetByIdAsync(id);

        // KIỂM TRA QUYỀN SỞ HỮU
        if (post == null || post.PartnerId != partnerId)
        {
            return Forbid(); // Trả về 403 Forbidden nếu cố sửa bài của người khác
        }

        try
        {
            post.Update(request.Title, request.Content, request.ImageUrl, request.RestaurantId);
            // TODO: Cập nhật lại Moods từ request.MoodIds

            await _postRepository.UpdateAsync(post);
            _logger.LogInformation("Partner {PartnerId} updated post {PostId}", partnerId, id);
            return Ok(ApiResponse.SuccessResult("Cập nhật bài đăng thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResult(ex.Message));
        }
    }

    /// <summary>
    /// Partner xóa bài đăng của mình
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeletePost(int id)
    {
        var partnerId = GetCurrentPartnerId();
        var post = await _postRepository.GetByIdAsync(id);

        // KIỂM TRA QUYỀN SỞ HỮU
        if (post == null || post.PartnerId != partnerId)
        {
            return Forbid();
        }
        
        // TODO: Xóa các comment, likes, PostMoods liên quan trước khi xóa post
        
        await _postRepository.DeleteAsync(id);
        _logger.LogInformation("Partner {PartnerId} deleted post {PostId}", partnerId, id);
        return Ok(ApiResponse.SuccessResult("Xóa bài đăng thành công."));
    }

    /// <summary>
    /// (Partner) Tải lên/Thay đổi ảnh cho bài đăng của mình
    /// </summary>
    [HttpPost("{id:int}/image")]
    public async Task<ActionResult<ApiResponse>> UploadPostImage(int id, IFormFile file)
    {
        var partnerId = GetCurrentPartnerId();
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null || post.PartnerId != partnerId)
        {
            return Forbid();
        }

        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.ErrorResult("Vui lòng chọn file ảnh."));
        
        // TODO: Nếu post đã có ảnh, nên xóa ảnh cũ trên Cloudinary trước

        await using var stream = file.OpenReadStream();
        var (url, publicId) = await _photoService.AddPhotoAsync(stream, file.FileName);

        if (url == null)
        {
            return BadRequest(ApiResponse.ErrorResult("Tải ảnh lên thất bại."));
        }

        post.SetImage(url);
        await _postRepository.UpdateAsync(post);
        _logger.LogInformation("Image uploaded for post {PostId} by partner {PartnerId}", id, partnerId);
        
        return Ok(ApiResponse<object>.SuccessResult(new { imageUrl = url }, "Tải ảnh lên thành công."));
    }

    /// <summary>
    /// (Partner) Gán một mood vào bài đăng của mình.
    /// </summary>
    [HttpPost("{postId:int}/moods/{moodId:int}")]
    public async Task<ActionResult<ApiResponse>> AddMoodToPost(int postId, int moodId)
    {
        var partnerId = GetCurrentPartnerId();
        var post = await _postRepository.GetByIdAsync(postId);

        // 1. Kiểm tra quyền sở hữu
        if (post == null || post.PartnerId != partnerId)
        {
            return Forbid();
        }

        // 2. Kiểm tra xem Mood có tồn tại không
        var mood = await _moodRepository.GetByIdAsync(moodId);
        if (mood == null)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy mood này."));
        }

        // 3. Gọi logic từ entity
        post.AddMood(moodId);

        // 4. Lưu thay đổi vào DB
        await _postRepository.UpdateAsync(post);
        _logger.LogInformation("Partner {PartnerId} added mood {MoodId} to post {PostId}", partnerId, moodId, postId);

        return Ok(ApiResponse.SuccessResult("Gán mood vào bài đăng thành công."));
    }

    /// <summary>
    /// (Partner) Gỡ một mood khỏi bài đăng của mình.
    /// </summary>
    [HttpDelete("{postId:int}/moods/{moodId:int}")]
    public async Task<ActionResult<ApiResponse>> RemoveMoodFromPost(int postId, int moodId)
    {
        var partnerId = GetCurrentPartnerId();
        var post = await _postRepository.GetByIdAsync(postId);

        // 1. Kiểm tra quyền sở hữu
        if (post == null || post.PartnerId != partnerId)
        {
            return Forbid();
        }

        // 2. Gọi logic từ entity
        post.RemoveMood(moodId);

        // 3. Lưu thay đổi vào DB
        await _postRepository.UpdateAsync(post);
        _logger.LogInformation("Partner {PartnerId} removed mood {MoodId} from post {PostId}", partnerId, moodId, postId);

        return Ok(ApiResponse.SuccessResult("Gỡ mood khỏi bài đăng thành công."));
    }

    /// <summary>
    /// Partner lấy chi tiết một bài đăng của chính mình theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> GetMyPostById(int id)
    {
        try
        {
            var partnerId = GetCurrentPartnerId();
            var post = await _postRepository.GetByIdAsync(id);

            // 1. Kiểm tra xem bài đăng có tồn tại không
            // 2. Kiểm tra xem bài đăng này có thuộc sở hữu của Partner đang đăng nhập không
            if (post == null || post.PartnerId != partnerId)
            {
                return NotFound(ApiResponse.ErrorResult("Không tìm thấy bài đăng."));
            }

            // Tái sử dụng phương thức map hiệu quả để lấy đầy đủ thông tin
            var dtos = await MapPostsToDtosAsync(new List<Post> { post });

            return Ok(ApiResponse<PostDto>.SuccessResult(dtos.First()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting post {PostId} for partner {PartnerId}", id, GetCurrentPartnerId());
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi tải chi tiết bài đăng."));
        }
    }


    // Tái sử dụng phương thức Map hiệu quả từ AdminPostsController
    private async Task<List<PostDto>> MapPostsToDtosAsync(IEnumerable<Post> posts)
    {
        if (!posts.Any())
        {
            return new List<PostDto>();
        }

        // 1. Thu thập tất cả các ID cần thiết một cách hiệu quả
        var postList = posts.ToList(); // Chuyển sang List để tránh lặp lại nhiều lần
        var partnerIds = postList.Select(p => p.PartnerId).Distinct();
        var restaurantIds = postList.Select(p => p.RestaurantId).Where(id => id.HasValue).Select(id => id!.Value).Distinct();
        var moodIds = postList.SelectMany(p => p.PostMoods).Select(pm => pm.MoodId).Distinct();

        // 2. Query dữ liệu liên quan song song để tăng hiệu năng
        var partnersTask = _userRepository.GetByIdsAsync(partnerIds);
        var restaurantsTask = _restaurantRepository.GetByIdsAsync(restaurantIds);
        var moodsTask = _moodRepository.GetByIdsAsync(moodIds);

        await Task.WhenAll(partnersTask, restaurantsTask, moodsTask);

        var partners = partnersTask.Result.ToDictionary(u => u.Id);
        var restaurants = restaurantsTask.Result.ToDictionary(r => r.Id);
        var moods = moodsTask.Result.ToDictionary(m => m.Id);

        // 3. Map sang DTO
        var dtos = postList.Select(p =>
        {
            // Map Partner
            partners.TryGetValue(p.PartnerId, out var partner);
            var partnerDto = partner != null
                ? new UserDto(partner.Id, partner.Email, partner.Fullname, partner.Role, partner.IsActive, partner.LoginMethod, partner.CreatedAt, null)
                : null;

            // Map Restaurant (SỬA LẠI ĐÂY)
            restaurants.TryGetValue(p.RestaurantId ?? 0, out var restaurant);
            var restaurantDto = restaurant != null
                ? new RestaurantDto(
                    restaurant.Id,
                    restaurant.PartnerId,
                    restaurant.Name,
                    restaurant.Address,
                    restaurant.Longitude,
                    restaurant.Latitude,
                    restaurant.Description,
                    restaurant.AvgPrice,
                    restaurant.Rating,
                    restaurant.Status,
                    restaurant.CreatedAt,
                    // Map danh sách ảnh của nhà hàng
                    restaurant.Images?.Select(img => new RestaurantImageDto(img.Id, restaurant.Id, img.ImageUrl, img.CreatedAt)).ToList() ?? new List<RestaurantImageDto>(),
                    new List<ReviewDto>(), // Tạm thời
                    0 // Tạm thời
                )
                : null;

            // Map Moods
            var postMoods = p.PostMoods
                .Select(pm => moods.TryGetValue(pm.MoodId, out var mood) ? new MoodDto(mood.Id, mood.Name, mood.Description, mood.CreatedAt) : null)
                .Where(m => m != null)
                .ToList();

            return new PostDto(
                p.Id, p.PartnerId, p.RestaurantId, p.Title, p.Content, p.ImageUrl,
                p.Status, // <<< THÊM p.Status VÀO ĐÂY
                p.CreatedAt,
                postMoods!,
                restaurantDto,
                partnerDto
            );
        }).ToList();

        return dtos;
    }
}
