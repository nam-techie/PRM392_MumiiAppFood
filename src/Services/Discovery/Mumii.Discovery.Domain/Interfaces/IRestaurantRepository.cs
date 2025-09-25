using Mumii.Discovery.Domain.Entities;
using Mumii.Shared.Common.DTOs;

namespace Mumii.Discovery.Domain.Interfaces;

/// <summary>
/// Repository interface cho Restaurant entity
/// </summary>
public interface IRestaurantRepository
{
    /// <summary>
    /// Tìm nhà hàng theo ID
    /// </summary>
    Task<Restaurant?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách nhà hàng có phân trang
    /// </summary>
    Task<PagedResult<Restaurant>> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm kiếm nhà hàng
    /// </summary>
    Task<PagedResult<Restaurant>> SearchAsync(
        SearchRestaurantsQuery query, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm nhà hàng gần vị trí
    /// </summary>
    Task<List<Restaurant>> GetNearbyAsync(
        NearbyRestaurantsQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm nhà hàng mới
    /// </summary>
    Task<Restaurant> AddAsync(Restaurant restaurant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật nhà hàng
    /// </summary>
    Task<Restaurant> UpdateAsync(Restaurant restaurant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa nhà hàng (soft delete)
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra nhà hàng có tồn tại không
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
