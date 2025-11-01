using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
// Removed conflicting domain interface usings to avoid ambiguity with Social.Domain.Interfaces

namespace Mumii.Social.Api.Controllers;

[ApiController]
[Route("api/admin/posts")]
[Authorize(Roles = "Admin")]
public class AdminPostsController : ControllerBase
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMoodRepository _moodRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<AdminPostsController> _logger;

    public AdminPostsController(
        IPostRepository postRepository,
        IUserRepository userRepository,
        IRestaurantRepository restaurantRepository,
        IMoodRepository moodRepository,
        ICommentRepository commentRepository,
        ILogger<AdminPostsController> logger)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _restaurantRepository = restaurantRepository;
        _moodRepository = moodRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PostDto>>>> GetAllPosts(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] int? partnerId = null)
    {
        var pagedPosts = await _postRepository.GetPagedAsync(page, pageSize, partnerId);
        var dtos = await MapPostsToDtosAsync(pagedPosts.Items);
        var result = new PagedResult<PostDto>(dtos.ToList(), pagedPosts.TotalCount, pagedPosts.Page, pagedPosts.PageSize, pagedPosts.TotalPages);
        return Ok(ApiResponse<PagedResult<PostDto>>.SuccessResult(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> GetPostById(int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy bài đăng."));

        var dtos = await MapPostsToDtosAsync(new List<Post> { post });
        return Ok(ApiResponse<PostDto>.SuccessResult(dtos.First()));
    }

    private async Task<List<PostDto>> MapPostsToDtosAsync(IEnumerable<Post> posts)
    {
        if (!posts.Any())
        {
            return new List<PostDto>();
        }

        // 1. Thu thập tất cả các ID cần thiết một cách hiệu quả
        var postList = posts.ToList(); // Chuyển sang List để tránh lặp lại nhiều lần
        var postIds = postList.Select(p => p.Id).Distinct().ToList();
        var partnerIds = postList.Select(p => p.PartnerId).Distinct();
        var restaurantIds = postList.Select(p => p.RestaurantId).Where(id => id.HasValue).Select(id => id!.Value).Distinct();
        var moodIds = postList.SelectMany(p => p.PostMoods).Select(pm => pm.MoodId).Distinct();

        // 2. Query dữ liệu liên quan song song để tăng hiệu năng
        var partnersTask = _userRepository.GetByIdsAsync(partnerIds);
        var restaurantsTask = _restaurantRepository.GetByIdsAsync(restaurantIds);
        var moodsTask = _moodRepository.GetByIdsAsync(moodIds);
        
        // Load comments cho tất cả posts song song
        var commentsTasks = postIds.Select(postId => _commentRepository.GetByPostIdAsync(postId));
        var allCommentsTask = Task.WhenAll(commentsTasks);

        await Task.WhenAll(partnersTask, restaurantsTask, moodsTask, allCommentsTask);

        var partners = partnersTask.Result.ToDictionary(u => u.Id);
        var restaurants = restaurantsTask.Result.ToDictionary(r => r.Id);
        var moods = moodsTask.Result.ToDictionary(m => m.Id);
        
        // Thu thập tất cả comments và group theo PostId
        var allComments = allCommentsTask.Result.SelectMany(c => c).ToList();
        var commentsByPostId = allComments.GroupBy(c => c.PostId).ToDictionary(g => g.Key, g => g.ToList());
        
        // Thu thập tất cả userIds từ comments và load users
        var commentUserIds = allComments.Select(c => c.UserId).Distinct().ToList();
        var commentUsers = commentUserIds.Any()
            ? (await _userRepository.GetByIdsAsync(commentUserIds)).ToDictionary(u => u.Id)
            : new Dictionary<int, UserDto>();

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

            // Map Comments
            var postComments = commentsByPostId.TryGetValue(p.Id, out var comments) && comments != null
                ? comments.Select(c =>
                {
                    commentUsers.TryGetValue(c.UserId, out var user);
                    var userDto = user != null
                        ? new UserDto(user.Id, user.Email, user.Fullname, user.Role, user.IsActive, user.LoginMethod, user.CreatedAt, null)
                        : null;
                    return new CommentDto(c.Id, c.PostId, c.UserId, c.Content, c.CreatedAt, userDto);
                }).ToList()
                : new List<CommentDto>();

            return new PostDto(
                p.Id, p.PartnerId, p.RestaurantId, p.Title, p.Content, p.ImageUrl,
                p.Status, // <<< THÊM p.Status VÀO ĐÂY
                p.CreatedAt,
                postMoods!,
                restaurantDto,
                partnerDto,
                postComments
            );
        }).ToList();

        return dtos;
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> UpdatePost(int id, [FromBody] AdminUpdatePostRequest request)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy bài đăng."));
        }

        try
        {
            post.Update(request.Title, request.Content, request.ImageUrl, request.RestaurantId);
            await _postRepository.UpdateAsync(post);
            _logger.LogInformation("Admin updated post {PostId}", id);
            return Ok(ApiResponse.SuccessResult("Cập nhật bài đăng thành công."));
        }
        catch (ArgumentException ex) 
        {
            return BadRequest(ApiResponse.ErrorResult(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeletePost(int id)
    {
        var postExists = await _postRepository.ExistsAsync(id);
        if (!postExists)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy bài đăng."));
        }
        
        await _postRepository.DeleteAsync(id);
        _logger.LogInformation("Admin deleted post {PostId}", id);
        return Ok(ApiResponse.SuccessResult("Xóa bài đăng thành công."));
    }

    /// <summary>
    /// (Admin) Duyệt một bài đăng
    /// </summary>
    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<ApiResponse>> ApprovePost(int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy bài đăng."));

        try
        {
            post.Approve();
            await _postRepository.UpdateAsync(post);
            _logger.LogInformation("Admin approved post {PostId}", id);
            // TODO: Gửi thông báo cho Partner
            return Ok(ApiResponse.SuccessResult("Duyệt bài đăng thành công."));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse.ErrorResult(ex.Message)); }
    }

    /// <summary>
    /// (Admin) Từ chối một bài đăng
    /// </summary>
    [HttpPost("{id:int}/decline")]
    public async Task<ActionResult<ApiResponse>> DeclinePost(int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy bài đăng."));

        try
        {
            post.Decline();
            await _postRepository.UpdateAsync(post);
            _logger.LogInformation("Admin declined post {PostId}", id);
            // TODO: Gửi thông báo cho Partner
            return Ok(ApiResponse.SuccessResult("Từ chối bài đăng thành công."));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse.ErrorResult(ex.Message)); }
    }
}

