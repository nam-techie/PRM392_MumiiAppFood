using System;
namespace Mumii.Shared.Common.DTOs;
public record NotificationDto(
    int Id,
    int UserId,
    string Title,
    string Content,
    bool IsRead,
    DateTime CreatedAt
);
