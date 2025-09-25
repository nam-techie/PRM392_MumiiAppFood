using Mumii.Shared.Common.Events;

namespace Mumii.Social.Domain.Entities;

/// <summary>
/// Entity bài đăng
/// </summary>
public class Post
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string AccountId { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? Mood { get; private set; }
    public List<string> ImageUrls { get; private set; } = new();
    public string? RestaurantId { get; private set; }
    public int ReactionCount { get; private set; } = 0;
    public int CommentCount { get; private set; } = 0;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; } = false;

    // Navigation properties
    public List<Comment> Comments { get; private set; } = new();
    public List<Reaction> Reactions { get; private set; } = new();

    // Domain events
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Constructor cho Entity Framework
    /// </summary>
    private Post() { }

    /// <summary>
    /// Tạo bài đăng mới
    /// </summary>
    public static Post Create(
        string accountId,
        string content,
        string? mood = null,
        List<string>? imageUrls = null,
        string? restaurantId = null)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID không được để trống", nameof(accountId));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung không được để trống", nameof(content));

        if (content.Length > 2000)
            throw new ArgumentException("Nội dung không được vượt quá 2000 ký tự", nameof(content));

        var post = new Post
        {
            AccountId = accountId,
            Content = content.Trim(),
            Mood = mood?.Trim(),
            ImageUrls = imageUrls ?? new List<string>(),
            RestaurantId = restaurantId?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add domain event
        post._domainEvents.Add(new PostCreatedEvent(
            post.Id,
            post.AccountId,
            post.Content,
            post.Mood,
            post.RestaurantId
        ));

        return post;
    }

    /// <summary>
    /// Cập nhật bài đăng
    /// </summary>
    public void Update(
        string content,
        string? mood = null,
        List<string>? imageUrls = null,
        string? restaurantId = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung không được để trống", nameof(content));

        if (content.Length > 2000)
            throw new ArgumentException("Nội dung không được vượt quá 2000 ký tự", nameof(content));

        Content = content.Trim();
        Mood = mood?.Trim();
        ImageUrls = imageUrls ?? ImageUrls;
        RestaurantId = restaurantId?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Thêm reaction
    /// </summary>
    public void AddReaction(string accountId, string reactionType)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID không được để trống", nameof(accountId));

        // Kiểm tra đã react chưa
        var existingReaction = Reactions.FirstOrDefault(r => r.AccountId == accountId);
        if (existingReaction != null)
        {
            // Nếu cùng loại reaction thì xóa (toggle)
            if (existingReaction.Type == reactionType)
            {
                RemoveReaction(accountId);
                return;
            }
            // Nếu khác loại thì update
            existingReaction.UpdateType(reactionType);
        }
        else
        {
            // Thêm reaction mới
            var reaction = Reaction.Create(Id, accountId, reactionType);
            Reactions.Add(reaction);
            ReactionCount++;

            _domainEvents.Add(new ReactionAddedEvent(Id, accountId, reactionType));
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Xóa reaction
    /// </summary>
    public void RemoveReaction(string accountId)
    {
        var reaction = Reactions.FirstOrDefault(r => r.AccountId == accountId);
        if (reaction != null)
        {
            Reactions.Remove(reaction);
            ReactionCount = Math.Max(0, ReactionCount - 1);
            UpdatedAt = DateTime.UtcNow;

            _domainEvents.Add(new ReactionRemovedEvent(Id, accountId, reaction.Type));
        }
    }

    /// <summary>
    /// Thêm comment
    /// </summary>
    public Comment AddComment(string accountId, string content, string? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID không được để trống", nameof(accountId));

        var comment = Comment.Create(Id, accountId, content, parentCommentId);
        Comments.Add(comment);
        CommentCount++;
        UpdatedAt = DateTime.UtcNow;

        _domainEvents.Add(new CommentAddedEvent(Id, comment.Id, accountId, content));

        return comment;
    }

    /// <summary>
    /// Xóa comment
    /// </summary>
    public void RemoveComment(string commentId)
    {
        var comment = Comments.FirstOrDefault(c => c.Id == commentId);
        if (comment != null)
        {
            comment.Delete();
            CommentCount = Math.Max(0, CommentCount - 1);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Soft delete bài đăng
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Khôi phục bài đăng
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Clear domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
