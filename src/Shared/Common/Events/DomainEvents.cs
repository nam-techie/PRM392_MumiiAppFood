using MediatR;

namespace Mumii.Shared.Common.Events;

/// <summary>
/// Base interface cho domain events
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base class cho domain events
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event khi user được tạo
/// </summary>
public record UserCreatedEvent(
    int UserId,
    string Email,
    string Fullname
) : DomainEvent;

/// <summary>
/// Event khi notification được tạo
/// </summary>
public record NotificationCreatedEvent(
    int NotificationId,
    int UserId,
    string Title,
    string Content
) : DomainEvent;

/// <summary>
/// Event khi nhà hàng được tạo
/// </summary>
public record RestaurantCreatedEvent(
    int RestaurantId,
    int PartnerId,
    string Name,
    string Address
) : DomainEvent;

/// <summary>
/// Event khi review được tạo
/// </summary>
public record ReviewCreatedEvent(
    int ReviewId,
    int UserId,
    int RestaurantId,
    int Rating
) : DomainEvent;

/// <summary>
/// Event khi review được cập nhật
/// </summary>
public record ReviewUpdatedEvent(
    int ReviewId,
    int UserId,
    int RestaurantId,
    int OldRating,
    int NewRating
) : DomainEvent;

/// <summary>
/// Event khi favorite được thêm
/// </summary>
public record FavoriteAddedEvent(
    int FavoriteId,
    int UserId,
    int RestaurantId
) : DomainEvent;

/// <summary>
/// Event khi bài đăng được tạo
/// </summary>
public record PostCreatedEvent(
    int PostId,
    int PartnerId,
    string Title,
    string Content,
    int? RestaurantId
) : DomainEvent;

/// <summary>
/// Event khi rating nhà hàng thay đổi
/// </summary>
public record RestaurantRatingUpdatedEvent(
    int RestaurantId,
    float NewRating,
    int TotalRatings
) : DomainEvent;

// Giữ lại các events cũ để backward compatibility
/// <summary>
/// Event khi tài khoản được tạo (legacy)
/// </summary>
public record AccountCreatedEvent(
    string AccountId,
    string Email,
    string DisplayName
) : DomainEvent;
