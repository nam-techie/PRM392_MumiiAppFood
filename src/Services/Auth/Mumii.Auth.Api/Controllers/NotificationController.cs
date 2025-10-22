using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Mumii.Auth.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize] // Tất cả các API trong đây đều yêu cầu đăng nhập
public class NotificationController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationRepository notificationRepository,
        ILogger<NotificationController> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    private bool TryGetUserId(out int userId)
    {
        userId = 0;
        var userIdStr = User.FindFirstValue("user_id"); // Lấy userId từ claim của token
        return !string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out userId);
    }
    
    /// <summary>
    /// Lấy danh sách tất cả thông báo của người dùng hiện tại
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetNotifications(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse.ErrorResult("Token không hợp lệ"));

        var notifications = await _notificationRepository.GetByUserIdAsync(userId, cancellationToken);

        var notificationDtos = notifications.Select(n => new NotificationDto(
            n.Id, n.UserId, n.Title, n.Content, n.IsRead, n.CreatedAt
        ));
        
        return Ok(ApiResponse<IEnumerable<NotificationDto>>.SuccessResult(notificationDtos));
    }

    /// <summary>
    /// Đánh dấu một thông báo là đã đọc
    /// </summary>
    /// <param name="id">ID của thông báo</param>
    [HttpPost("{id:int}/read")]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse.ErrorResult("Token không hợp lệ"));

        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);

        // Kiểm tra xem notification có tồn tại và có thuộc về user này không
        if (notification == null || notification.UserId != userId)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy thông báo"));
        }
        
        if (notification.IsRead) // Nếu đã đọc rồi thì không cần update
            return Ok(ApiResponse.SuccessResult("Thông báo đã được đánh dấu đọc"));

        notification.MarkAsRead();
        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        _logger.LogInformation("Notification {NotificationId} marked as read for user {UserId}", id, userId);

        return Ok(ApiResponse.SuccessResult("Đánh dấu đọc thành công"));
    }
    
    /// <summary>
    /// Đánh dấu tất cả thông báo là đã đọc
    /// </summary>
    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse>> MarkAllAsRead(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse.ErrorResult("Token không hợp lệ"));

        var notifications = (await _notificationRepository.GetByUserIdAsync(userId, cancellationToken))
            .Where(n => !n.IsRead).ToList();

        if (!notifications.Any())
        {
            return Ok(ApiResponse.SuccessResult("Không có thông báo mới để đánh dấu"));
        }

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await _notificationRepository.UpdateManyAsync(notifications, cancellationToken);
        _logger.LogInformation("All notifications marked as read for user {UserId}", userId);
        
        return Ok(ApiResponse.SuccessResult("Tất cả thông báo đã được đánh dấu đọc"));
    }

    /// <summary>
    /// Xóa một thông báo
    /// </summary>
    /// <param name="id">ID của thông báo</param>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteNotification(int id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse.ErrorResult("Token không hợp lệ"));
            
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);

        // Kiểm tra xem notification có tồn tại và có thuộc về user này không
        if (notification == null || notification.UserId != userId)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy thông báo"));
        }
        
        await _notificationRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Notification {NotificationId} deleted for user {UserId}", id, userId);

        return Ok(ApiResponse.SuccessResult("Xóa thông báo thành công"));
    }
}
