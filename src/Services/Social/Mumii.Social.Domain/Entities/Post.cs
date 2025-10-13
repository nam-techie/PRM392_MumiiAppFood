using Mumii.Shared.Common.Events;

namespace Mumii.Social.Domain.Entities;

/// <summary>
/// Entity bài đăng theo schema mới
/// </summary>
public class Post
{
    public int Id { get; private set; }
    public int PartnerId { get; private set; }
    public int? RestaurantId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation properties
    public List<PostMood> PostMoods { get; private set; } = new();
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
        int partnerId,
        string title,
        string content,
        int? restaurantId = null)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề không được để trống", nameof(title));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung không được để trống", nameof(content));

        if (title.Length > 255)
            throw new ArgumentException("Tiêu đề không được vượt quá 255 ký tự", nameof(title));

        if (content.Length > 2000)
            throw new ArgumentException("Nội dung không được vượt quá 2000 ký tự", nameof(content));

        var post = new Post
        {
            PartnerId = partnerId,
            Title = title.Trim(),
            Content = content.Trim(),
            RestaurantId = restaurantId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add domain event
        post._domainEvents.Add(new PostCreatedEvent(
            post.Id,
            post.PartnerId,
            post.Title,
            post.Content,
            post.RestaurantId
        ));

        return post;
    }

    /// <summary>
    /// Cập nhật bài đăng
    /// </summary>
    public void Update(
        string title,
        string content,
        int? restaurantId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề không được để trống", nameof(title));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung không được để trống", nameof(content));

        if (title.Length > 255)
            throw new ArgumentException("Tiêu đề không được vượt quá 255 ký tự", nameof(title));

        if (content.Length > 2000)
            throw new ArgumentException("Nội dung không được vượt quá 2000 ký tự", nameof(content));

        Title = title.Trim();
        Content = content.Trim();
        RestaurantId = restaurantId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Thêm mood cho post
    /// </summary>
    public void AddMood(int moodId)
    {
        // Kiểm tra đã có mood này chưa
        var existingMood = PostMoods.FirstOrDefault(pm => pm.MoodId == moodId);
        if (existingMood != null)
                return;

        var postMood = PostMood.Create(Id, moodId);
        PostMoods.Add(postMood);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Xóa mood khỏi post
    /// </summary>
    public void RemoveMood(int moodId)
    {
        var postMood = PostMoods.FirstOrDefault(pm => pm.MoodId == moodId);
        if (postMood != null)
        {
            PostMoods.Remove(postMood);
            UpdatedAt = DateTime.UtcNow;
        }
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
