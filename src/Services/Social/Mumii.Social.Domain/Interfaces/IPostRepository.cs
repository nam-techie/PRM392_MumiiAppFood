using Mumii.Shared.Common.Models;
using Mumii.Social.Domain.Entities;
using Mumii.Shared.Common.DTOs;
using System.Threading.Tasks;
using System.Threading;

namespace Mumii.Social.Domain.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<Post>> GetPagedAsync(
        int page, 
        int pageSize,
        int? partnerId = null,
        string? status = null, 
        int? restaurantId = null, 
        CancellationToken cancellationToken = default);

    Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default);
    Task UpdateAsync(Post post, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<Post>> SearchAsync(SearchPostsQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<Post>> GetByAccountIdAsync(int accountId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem có bất kỳ bài đăng nào đang sử dụng moodId này không
    /// </summary>
    Task<bool> IsMoodInUseAsync(int moodId, CancellationToken cancellationToken = default);
}
