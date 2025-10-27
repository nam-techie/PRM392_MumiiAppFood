using Mumii.Discovery.Domain.Entities;
using Mumii.Shared.Common.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Mumii.Discovery.Domain.Interfaces;

public interface IReviewRepository
{
    /// <summary>
    /// Tìm review theo ID
    /// </summary>
    Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách review của một nhà hàng, có phân trang
    /// </summary>
    Task<PagedResult<Review>> GetByRestaurantIdAsync(int restaurantId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật một review (ví dụ: thêm/sửa reply)
    /// </summary>
    Task UpdateAsync(Review review, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm một review mới (do user tạo)
    /// </summary>
    Task<Review> AddAsync(Review review, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một review (do user hoặc admin thực hiện)
    /// </summary>
    Task DeleteAsync(int reviewId, CancellationToken cancellationToken = default);
}