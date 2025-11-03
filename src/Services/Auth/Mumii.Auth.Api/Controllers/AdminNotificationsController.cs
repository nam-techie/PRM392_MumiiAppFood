using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Services;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Mumii.Auth.Api.Controllers;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = "Admin")]
public class AdminNotificationsController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository; // Cần để lấy danh sách user khi broadcast
    private readonly IMongoIdGenerator _idGenerator;
    private readonly ILogger<AdminNotificationsController> _logger;

    public AdminNotificationsController(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IMongoIdGenerator idGenerator,
        ILogger<AdminNotificationsController> logger)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _idGenerator = idGenerator;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả thông báo, có phân trang và lọc theo user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> GetNotifications(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? userId = null)
    {
        var pagedResult = await _notificationRepository.GetPagedAsync(page, pageSize, userId);
        
        var dtos = pagedResult.Items.Select(n => new NotificationDto(n.Id, n.UserId, n.Title, n.Content, n.IsRead, n.CreatedAt)).ToList();
        
        var result = new PagedResult<NotificationDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalPages);

        return Ok(ApiResponse<PagedResult<NotificationDto>>.SuccessResult(result));
    }

    /// <summary>
    /// Gửi thông báo đến một người dùng cụ thể
    /// </summary>
    [HttpPost("send-to-user")]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> SendToUser([FromBody] CreateNotificationRequest request)
    {
        var userExists = await _userRepository.GetByIdAsync(request.UserId) != null;
        if (!userExists)
        {
            return NotFound(ApiResponse.ErrorResult($"Không tìm thấy người dùng với ID {request.UserId}."));
        }
        
        var newId = await _idGenerator.GetNextIdAsync("notifications");
        var notification = Notification.Create(newId, request.UserId, request.Title, request.Content);

        await _notificationRepository.AddAsync(notification);
        _logger.LogInformation("Admin sent notification {NotificationId} to user {UserId}", newId, request.UserId);

        var dto = new NotificationDto(notification.Id, notification.UserId, notification.Title, notification.Content, notification.IsRead, notification.CreatedAt);
        return Ok(ApiResponse<NotificationDto>.SuccessResult(dto, "Gửi thông báo thành công."));
    }

    /// <summary>
    /// Gửi thông báo đến tất cả người dùng
    /// </summary>
    [HttpPost("broadcast")]
    public async Task<ActionResult<ApiResponse>> Broadcast([FromBody] BroadcastNotificationRequest request)
    {
        // Tạm thời lấy 1000 user đầu tiên, cần logic phân trang nếu có nhiều user hơn
        var users = await _userRepository.GetAllAsync(limit: 1000); 

        var notifications = new List<Notification>();
        foreach (var user in users)
        {
            var newId = await _idGenerator.GetNextIdAsync("notifications");
            var notification = Notification.Create(newId, user.Id, request.Title, request.Content);
            notifications.Add(notification);
        }

        await _notificationRepository.AddManyAsync(notifications);
        _logger.LogInformation("Admin broadcasted a notification to {UserCount} users.", users.Count());

        return Ok(ApiResponse.SuccessResult($"Đã gửi thông báo đến {users.Count()} người dùng."));
    }

    /// <summary>
    /// Cập nhật một thông báo
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> UpdateNotification(int id, [FromBody] UpdateNotificationRequest request)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy thông báo."));
        }
        
        notification.Update(request.Title, request.Content);
        await _notificationRepository.UpdateAsync(notification);
        _logger.LogInformation("Admin updated notification {NotificationId}", id);

        return Ok(ApiResponse.SuccessResult("Cập nhật thông báo thành công."));
    }

    /// <summary>
    /// Xóa một thông báo
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteNotification(int id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
        {
            return NotFound(ApiResponse.ErrorResult("Không tìm thấy thông báo."));
        }

        await _notificationRepository.DeleteAsync(id);
        _logger.LogInformation("Admin deleted notification {NotificationId}", id);

        return Ok(ApiResponse.SuccessResult("Xóa thông báo thành công."));
    }
}
