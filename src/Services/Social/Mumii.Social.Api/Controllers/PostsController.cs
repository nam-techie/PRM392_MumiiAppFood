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
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<PostsController> _logger;

    public PostsController(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        ILogger<PostsController> logger)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách bài đăng
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PostDto>>>> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? mood = null,
        [FromQuery] string? accountId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            PagedResult<Post> result;

            if (!string.IsNullOrWhiteSpace(mood) || !string.IsNullOrWhiteSpace(accountId))
            {
                var query = new SearchPostsQuery(
                    Mood: mood,
                    RestaurantId: null,
                    AccountId: accountId,
                    FromDate: null,
                    ToDate: null,
                    Page: page,
                    PageSize: pageSize
                );
                result = await _postRepository.SearchAsync(query, cancellationToken);
            }
            else
            {
                result = await _postRepository.GetPagedAsync(page, pageSize, cancellationToken);
            }
            
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
            // Tạm thời sử dụng account ID cố định, trong thực tế sẽ lấy từ JWT token
            var accountId = "demo-account-id";

            var post = Post.Create(
                accountId: accountId,
                content: request.Content,
                mood: request.Mood,
                imageUrls: request.ImageUrls,
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
                content: request.Content,
                mood: request.Mood,
                imageUrls: request.ImageUrls,
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
    /// Toggle reaction cho bài đăng
    /// </summary>
    [HttpPut("{id}/react")]
    public async Task<ActionResult<ApiResponse>> ToggleReaction(
        string id,
        [FromBody] ToggleReactionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Tạm thời sử dụng account ID cố định
            var accountId = "demo-account-id";

            var post = await _postRepository.GetByIdAsync(id, cancellationToken);
            if (post == null)
            {
                return NotFound(ApiResponse.ErrorResult(
                    "Không tìm thấy",
                    "Bài đăng không tồn tại"));
            }

            post.AddReaction(accountId, request.Type);
            await _postRepository.UpdateAsync(post, cancellationToken);
            await _postRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reaction toggled for post: {PostId}", id);
            return Ok(ApiResponse.SuccessResult("Reaction đã được cập nhật"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling reaction for post {PostId}", id);
            return StatusCode(500, ApiResponse.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi cập nhật reaction"));
        }
    }

    /// <summary>
    /// Lấy comments của bài đăng
    /// </summary>
    [HttpGet("{id}/comments")]
    public async Task<ActionResult<ApiResponse<List<CommentDto>>>> GetComments(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _postRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
            {
                return NotFound(ApiResponse<List<CommentDto>>.ErrorResult(
                    "Không tìm thấy",
                    "Bài đăng không tồn tại"));
            }

            var comments = await _commentRepository.GetByPostIdAsync(id, cancellationToken);
            var commentDtos = comments.Select(MapCommentToDto).ToList();

            return Ok(ApiResponse<List<CommentDto>>.SuccessResult(commentDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for post {PostId}", id);
            return StatusCode(500, ApiResponse<List<CommentDto>>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi lấy danh sách comments"));
        }
    }

    /// <summary>
    /// Thêm comment cho bài đăng
    /// </summary>
    [HttpPost("{id}/comments")]
    public async Task<ActionResult<ApiResponse<CommentDto>>> CreateComment(
        string id,
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Tạm thời sử dụng account ID cố định
            var accountId = "demo-account-id";

            var post = await _postRepository.GetByIdAsync(id, cancellationToken);
            if (post == null)
            {
                return NotFound(ApiResponse<CommentDto>.ErrorResult(
                    "Không tìm thấy",
                    "Bài đăng không tồn tại"));
            }

            var comment = post.AddComment(accountId, request.Content, request.ParentCommentId);
            await _postRepository.UpdateAsync(post, cancellationToken);
            await _postRepository.SaveChangesAsync(cancellationToken);

            var commentDto = MapCommentToDto(comment);
            
            _logger.LogInformation("Comment created for post: {PostId}", id);
            return Ok(ApiResponse<CommentDto>.SuccessResult(commentDto, "Tạo comment thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<CommentDto>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment for post {PostId}", id);
            return StatusCode(500, ApiResponse<CommentDto>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi tạo comment"));
        }
    }

    /// <summary>
    /// Map Post entity to DTO
    /// </summary>
    private static PostDto MapToDto(Post post)
    {
        // Tạm thời tạo fake account data
        var account = new AccountDto(
            "demo-account-id",
            "demo@mumii.com",
            "Demo User",
            null,
            "User",
            true,
            DateTime.UtcNow
        );

        return new PostDto(
            Id: post.Id,
            AccountId: post.AccountId,
            Content: post.Content,
            Mood: post.Mood,
            ImageUrls: post.ImageUrls,
            RestaurantId: post.RestaurantId,
            ReactionCount: post.ReactionCount,
            CommentCount: post.CommentCount,
            CreatedAt: post.CreatedAt,
            Account: account,
            Restaurant: null, // Sẽ cần gọi Discovery Service để lấy thông tin restaurant
            UserReaction: null // Sẽ cần check reaction của user hiện tại
        );
    }

    /// <summary>
    /// Map Comment entity to DTO
    /// </summary>
    private static CommentDto MapCommentToDto(Comment comment)
    {
        // Tạm thời tạo fake account data
        var account = new AccountDto(
            comment.AccountId,
            "demo@mumii.com",
            "Demo User",
            null,
            "User",
            true,
            DateTime.UtcNow
        );

        var replies = comment.Replies.Select(MapCommentToDto).ToList();

        return new CommentDto(
            Id: comment.Id,
            PostId: comment.PostId,
            AccountId: comment.AccountId,
            Content: comment.Content,
            ParentCommentId: comment.ParentCommentId,
            CreatedAt: comment.CreatedAt,
            Account: account,
            Replies: replies
        );
    }
}
