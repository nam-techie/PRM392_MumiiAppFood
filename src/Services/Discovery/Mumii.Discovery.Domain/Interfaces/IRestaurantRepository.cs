using Mumii.Discovery.Domain.Entities;
using Mumii.Shared.Common.DTOs;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace Mumii.Discovery.Domain.Interfaces;

/// <summary>
/// Repository interface cho Restaurant entity
/// </summary>
public interface IRestaurantRepository
{
    /// <summary>
    /// Tìm nhà hàng theo ID
    /// </summary>
    Task<Restaurant?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

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
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra nhà hàng có tồn tại không
    /// </summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách nhà hàng theo Partner ID
    /// </summary>
    Task<List<Restaurant>> GetByPartnerIdAsync(int partnerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách nhà hàng có phân trang theo status
    /// </summary>
    Task<PagedResult<Restaurant>> GetPagedByStatusAsync(int page, int pageSize, string? status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách nhà hàng theo danh sách ID
    /// </summary>
    Task<IEnumerable<Restaurant>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
