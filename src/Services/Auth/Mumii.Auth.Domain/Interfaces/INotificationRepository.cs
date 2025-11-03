using Mumii.Auth.Domain.Entities;
using Mumii.Shared.Common.Models; // Sửa namespace nếu PagedResult ở nơi khác
using Mumii.Shared.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mumii.Auth.Domain.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateManyAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    // >>> THÊM CÁC PHƯƠNG THỨC MỚI CHO ADMIN <<<
    /// <summary>
    /// Lấy danh sách tất cả thông báo có phân trang
    /// </summary>
    Task<PagedResult<Notification>> GetPagedAsync(int page, int pageSize, int? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm nhiều thông báo cùng lúc (dùng cho broadcast)
    /// </summary>
    Task AddManyAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
}