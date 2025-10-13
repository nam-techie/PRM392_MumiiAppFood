using Mumii.Auth.Domain.Entities;

namespace Mumii.Auth.Domain.Interfaces;

/// <summary>
/// Repository interface cho Notification entity (MongoDB)
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Tìm notification theo ID
    /// </summary>
    Task<Notification?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả notifications của user
    /// </summary>
    Task<List<Notification>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy notifications chưa đọc của user
    /// </summary>
    Task<List<Notification>> GetUnreadByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm notification mới
    /// </summary>
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật notification
    /// </summary>
    Task<Notification> UpdateAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa notification
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

