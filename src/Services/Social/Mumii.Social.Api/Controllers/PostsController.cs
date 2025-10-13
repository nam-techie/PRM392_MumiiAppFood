using Microsoft.AspNetCore.Mvc;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using Mumii.Shared.Common.Constants;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;

namespace Mumii.Social.Api.Controllers;

/// <summary>
/// Controller xử lý social posts
/// </summary>
[ApiController]
[Route(ApiRoutes.Social.Base)]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _postRepository;
    private readonly ILogger<PostsController> _logger;

    public PostsController(
        IPostRepository postRepository,
        ILogger<PostsController> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách bài đăng
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PostDto>>>> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? partnerId = null,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = new SearchPostsQuery(
                MoodIds: null,
                RestaurantId: restaurantId,
                PartnerId: partnerId,
                FromDate: null,
                ToDate: null,
                Page: page,
                PageSize: pageSize
            );
            var result = await _postRepository.SearchAsync(query, cancellationToken);
            
            var postDtos = result.Items.Select(MapToDto).ToList();
            var pagedResult = new PagedResult<PostDto>(
                postDtos,
                result.TotalCount,
                result.Page,
                result.PageSize,
                result.TotalPages
            );

            return Ok(ApiResponse<PagedResult<PostDto>>.SuccessResult(pagedResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting posts");
            return StatusCode(500, ApiResponse<PagedResult<PostDto>>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi lấy danh sách bài đăng"));
        }
    }

    /// <summary>
    /// Lấy thông tin bài đăng theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> GetPost(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(id, cancellationToken);
            if (post == null)
            {
                return NotFound(ApiResponse<PostDto>.ErrorResult(
                    "Không tìm thấy",
                    "Bài đăng không tồn tại"));
            }

            var postDto = MapToDto(post);
            return Ok(ApiResponse<PostDto>.SuccessResult(postDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting post {PostId}", id);
            return StatusCode(500, ApiResponse<PostDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi lấy thông tin bài đăng"));
        }
    }

    /// <summary>
    /// Tạo bài đăng mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PostDto>>> CreatePost(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var partnerId = 1; // TODO: lấy từ JWT sau
            var post = Post.Create(
                id: 0,
                partnerId: partnerId,
                title: request.Title,
                content: request.Content,
                imageUrl: request.ImageUrl,
                restaurantId: request.RestaurantId
            );

            await _postRepository.AddAsync(post, cancellationToken);
            await _postRepository.SaveChangesAsync(cancellationToken);

            var postDto = MapToDto(post);
            
            _logger.LogInformation("Post created: {PostId}", post.Id);
            return CreatedAtAction(
                nameof(GetPost), 
                new { id = post.Id }, 
                ApiResponse<PostDto>.SuccessResult(postDto, "Tạo bài đăng thành công"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Post creation validation failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<PostDto>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            return StatusCode(500, ApiResponse<PostDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi trong quá trình tạo bài đăng"));
        }
    }

    /// <summary>
    /// Cập nhật bài đăng
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> UpdatePost(
        string id,
        [FromBody] UpdatePostRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(id, cancellationToken);
            if (post == null)
            {
                return NotFound(ApiResponse<PostDto>.ErrorResult(
                    "Không tìm thấy",
                    "Bài đăng không tồn tại"));
            }

            post.Update(
                title: request.Title,
                content: request.Content,
                imageUrl: request.ImageUrl,
                restaurantId: request.RestaurantId
            );

            await _postRepository.UpdateAsync(post, cancellationToken);
            await _postRepository.SaveChangesAsync(cancellationToken);

            var postDto = MapToDto(post);
            
            _logger.LogInformation("Post updated: {PostId}", post.Id);
            return Ok(ApiResponse<PostDto>.SuccessResult(postDto, "Cập nhật bài đăng thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PostDto>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post {PostId}", id);
            return StatusCode(500, ApiResponse<PostDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi cập nhật bài đăng"));
        }
    }

    /// <summary>
    /// Xóa bài đăng
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeletePost(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _postRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
            {
                return NotFound(ApiResponse.ErrorResult(
                    "Không tìm thấy",
                    "Bài đăng không tồn tại"));
            }

            await _postRepository.DeleteAsync(id, cancellationToken);
            await _postRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Post deleted: {PostId}", id);
            return Ok(ApiResponse.SuccessResult("Xóa bài đăng thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", id);
            return StatusCode(500, ApiResponse.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi xóa bài đăng"));
        }
    }

    /// <summary>
    /// Map Post entity to DTO
    /// </summary>
    private static PostDto MapToDto(Post post)
    {
        return new PostDto(
            Id: post.Id,
            PartnerId: post.PartnerId,
            RestaurantId: post.RestaurantId,
            Title: post.Title,
            Content: post.Content,
            ImageUrl: post.ImageUrl,
            CreatedAt: post.CreatedAt,
            Moods: new List<MoodDto>(),
            Restaurant: null,
            Partner: new UserDto(0, "", "", "User", true, "", DateTime.UtcNow, null)
        );
    }
}
