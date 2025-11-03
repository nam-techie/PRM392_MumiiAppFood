using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Services;
using Mumii.Social.Domain;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
// Removed conflicting domain interface usings to avoid ambiguity with Social.Domain.Interfaces
using System;
using Microsoft.Extensions.Logging;

namespace Mumii.Social.Api.Controllers;

[ApiController]
[Route("api/posts")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<PostsController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMoodRepository _moodRepository;
    private readonly IMongoIdGenerator _idGenerator; // Cần để tạo ID cho comment

    public PostsController(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        ILogger<PostsController> logger,
        IUserRepository userRepository,
        IRestaurantRepository restaurantRepository,
        IMoodRepository moodRepository,
        IMongoIdGenerator idGenerator)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _logger = logger;
        _userRepository = userRepository;
        _restaurantRepository = restaurantRepository;
        _moodRepository = moodRepository;
        _idGenerator = idGenerator;
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        var userIdStr = User.FindFirstValue("user_id");
        return !string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out userId);
    }

    /// <summary>
    /// (Public) Lấy danh sách bài đăng (newsfeed)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PostDto>>>> GetPosts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            // Luôn chỉ lấy các bài đã duyệt cho newsfeed
            var pagedPosts = await _postRepository.GetPagedAsync(page, pageSize, status: "Approved");
            var dtos = await MapPostsToDtosAsync(pagedPosts.Items);
            var result = new PagedResult<PostDto>(dtos.ToList(), pagedPosts.TotalCount, pagedPosts.Page, pagedPosts.PageSize, pagedPosts.TotalPages);
            return Ok(ApiResponse<PagedResult<PostDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting posts for newsfeed.");
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi tải bài đăng."));
        }
    }

    /// <summary>
    /// (Public) Lấy danh sách bình luận của một bài đăng
    /// </summary>
    [HttpGet("{postId:int}/comments")]
    [Authorize] // Cần token để xem comments
    public async Task<ActionResult<ApiResponse<List<CommentDto>>>> GetCommentsForPost(int postId)
    {
        try
        {
            var comments = await _commentRepository.GetByPostIdAsync(postId);

            if (!comments.Any())
            {
                return Ok(ApiResponse<List<CommentDto>>.SuccessResult(new List<CommentDto>()));
            }

            var userIds = comments.Select(c => c.UserId).Distinct().ToList();
            var users = (await _userRepository.GetByIdsAsync(userIds)).ToDictionary(u => u.Id);

            var dtos = comments.Select(c =>
            {
                users.TryGetValue(c.UserId, out var user);
                var userDto = user != null ? new UserDto(user.Id, user.Email, user.Fullname, user.Role, user.IsActive, user.LoginMethod, user.CreatedAt, null) : null;
                return new CommentDto(c.Id, c.PostId, c.UserId, c.Content, c.CreatedAt, userDto);
            }).ToList();

            return Ok(ApiResponse<List<CommentDto>>.SuccessResult(dtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for post {PostId}", postId);
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi tải bình luận."));
        }
    }

    /// <summary>
    /// (Public) Lấy chi tiết một bài đăng
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> GetPostById(int id)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(id);
            // Chỉ trả về nếu post tồn tại VÀ đã được duyệt
            if (post == null || post.Status != "Approved")
            {
                return NotFound(ApiResponse.ErrorResult("Không tìm thấy bài đăng."));
            }
            var dtos = await MapPostsToDtosAsync(new List<Post> { post });
            return Ok(ApiResponse<PostDto>.SuccessResult(dtos.First()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting post by id {PostId}", id);
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi tải chi tiết bài đăng."));
        }
    }

    /// <summary>
    /// (User) Thêm một bình luận mới vào bài đăng
    /// </summary>
    [HttpPost("{postId:int}/comments")]
    [Authorize] // Chỉ user đã đăng nhập mới được bình luận
    public async Task<ActionResult<ApiResponse<CommentDto>>> AddComment(int postId, [FromBody] CreateCommentRequest request)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized(ApiResponse.ErrorResult("Token không hợp lệ."));

            var postExists = await _postRepository.ExistsAsync(postId);
            if (!postExists)
            {
                return NotFound(ApiResponse.ErrorResult("Không tìm thấy bài đăng để bình luận."));
            }
            
            var newId = await _idGenerator.GetNextIdAsync("comments");
            var comment = Comment.Create(newId, postId, userId, request.Content);
            
            var createdComment = await _commentRepository.AddAsync(comment);
            
            var user = await _userRepository.GetByIdAsync(userId);
            var userDto = user != null ? new UserDto(user.Id, user.Email, user.Fullname, user.Role, user.IsActive, user.LoginMethod, user.CreatedAt, null) : null;
            
            var dto = new CommentDto(createdComment.Id, createdComment.PostId, createdComment.UserId, createdComment.Content, createdComment.CreatedAt, userDto);
            
            _logger.LogInformation("User {UserId} commented on post {PostId}", userId, postId);
            return Ok(ApiResponse<CommentDto>.SuccessResult(dto, "Bình luận thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error adding comment for post {PostId}", postId);
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi thêm bình luận."));
        }
    }

    /// <summary>
    /// (Public) Lấy danh sách bài đăng của một nhà hàng cụ thể
    /// </summary>
    [HttpGet("by-restaurant/{restaurantId:int}")]
    public async Task<ActionResult<ApiResponse<PagedResult<PostDto>>>> GetPostsByRestaurant(
        int restaurantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var pagedPosts = await _postRepository.GetPagedAsync(
                page: page, 
                pageSize: pageSize, 
                status: "Approved", // Chỉ lấy bài đã duyệt
                restaurantId: restaurantId // Lọc theo nhà hàng
            );
            
            var dtos = await MapPostsToDtosAsync(pagedPosts.Items);
            var result = new PagedResult<PostDto>(dtos.ToList(), pagedPosts.TotalCount, pagedPosts.Page, pagedPosts.PageSize, pagedPosts.TotalPages);
            
            return Ok(ApiResponse<PagedResult<PostDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting posts for restaurant {RestaurantId}", restaurantId);
            return StatusCode(500, ApiResponse.ErrorResult("Lỗi hệ thống khi tải bài đăng."));
        }
    }

    // Phương thức helper để map Post sang PostDto, tái sử dụng logic
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
}