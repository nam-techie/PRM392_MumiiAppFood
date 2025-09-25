using Mumii.Social.Domain.Entities;
using Mumii.Shared.Common.DTOs;

namespace Mumii.Social.Domain.Interfaces;

/// <summary>
/// Repository interface cho Post entity
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Tìm bài đăng theo ID
    /// </summary>
    Task<Post?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách bài đăng có phân trang
    /// </summary>
    Task<PagedResult<Post>> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm kiếm bài đăng
    /// </summary>
    Task<PagedResult<Post>> SearchAsync(
        SearchPostsQuery query, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy bài đăng của một user
    /// </summary>
    Task<PagedResult<Post>> GetByAccountIdAsync(
        string accountId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm bài đăng mới
    /// </summary>
    Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật bài đăng
    /// </summary>
    Task<Post> UpdateAsync(Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa bài đăng (soft delete)
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra bài đăng có tồn tại không
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface cho Comment entity
/// </summary>
public interface ICommentRepository
{
    /// <summary>
    /// Tìm comment theo ID
    /// </summary>
    Task<Comment?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy comments của một post
    /// </summary>
    Task<List<Comment>> GetByPostIdAsync(string postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm comment mới
    /// </summary>
    Task<Comment> AddAsync(Comment comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật comment
    /// </summary>
    Task<Comment> UpdateAsync(Comment comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa comment (soft delete)
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
