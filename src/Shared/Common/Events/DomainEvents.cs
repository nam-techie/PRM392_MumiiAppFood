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
/// Event khi tài khoản được tạo
/// </summary>
public record AccountCreatedEvent(
    string AccountId,
    string Email,
    string DisplayName
) : DomainEvent;

/// <summary>
/// Event khi nhà hàng được tạo
/// </summary>
public record RestaurantCreatedEvent(
    string RestaurantId,
    string Name,
    string Address,
    decimal? Latitude,
    decimal? Longitude
) : DomainEvent;

/// <summary>
/// Event khi bài đăng được tạo
/// </summary>
public record PostCreatedEvent(
    string PostId,
    string AccountId,
    string Content,
    string? Mood,
    string? RestaurantId
) : DomainEvent;

/// <summary>
/// Event khi có reaction mới
/// </summary>
public record ReactionAddedEvent(
    string PostId,
    string AccountId,
    string ReactionType
) : DomainEvent;

/// <summary>
/// Event khi reaction bị xóa
/// </summary>
public record ReactionRemovedEvent(
    string PostId,
    string AccountId,
    string ReactionType
) : DomainEvent;

/// <summary>
/// Event khi có comment mới
/// </summary>
public record CommentAddedEvent(
    string PostId,
    string CommentId,
    string AccountId,
    string Content
) : DomainEvent;

/// <summary>
/// Event khi rating nhà hàng thay đổi
/// </summary>
public record RestaurantRatingUpdatedEvent(
    string RestaurantId,
    decimal NewRating,
    int TotalRatings
) : DomainEvent;
