namespace Mumii.Social.Domain.Entities;

/// <summary>
/// Entity comment
/// </summary>
public class Comment
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string PostId { get; private set; } = string.Empty;
    public string AccountId { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? ParentCommentId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; } = false;

    // Navigation properties
    public Post Post { get; private set; } = null!;
    public Comment? ParentComment { get; private set; }
    public List<Comment> Replies { get; private set; } = new();

    /// <summary>
    /// Constructor cho Entity Framework
    /// </summary>
    private Comment() { }

    /// <summary>
    /// Tạo comment mới
    /// </summary>
    public static Comment Create(
        string postId,
        string accountId,
        string content,
        string? parentCommentId = null)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(postId))
            throw new ArgumentException("Post ID không được để trống", nameof(postId));
        
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID không được để trống", nameof(accountId));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung không được để trống", nameof(content));

        if (content.Length > 1000)
            throw new ArgumentException("Nội dung không được vượt quá 1000 ký tự", nameof(content));

        return new Comment
        {
            PostId = postId,
            AccountId = accountId,
            Content = content.Trim(),
            ParentCommentId = parentCommentId?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cập nhật comment
    /// </summary>
    public void Update(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung không được để trống", nameof(content));

        if (content.Length > 1000)
            throw new ArgumentException("Nội dung không được vượt quá 1000 ký tự", nameof(content));

        Content = content.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft delete comment
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Khôi phục comment
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
