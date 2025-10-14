namespace Mumii.Social.Domain.Entities;

/// <summary>
/// Comment entity (Mongo-oriented minimal definition)
/// </summary>
public class Comment
{
    public string Id { get; private set; } = string.Empty;
    public string PostId { get; private set; } = string.Empty;
    public string AccountId { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; }
        = false;

    private Comment() { }

    public static Comment Create(string postId, string accountId, string content)
    {
        if (string.IsNullOrWhiteSpace(postId)) throw new ArgumentException("postId is required", nameof(postId));
        if (string.IsNullOrWhiteSpace(accountId)) throw new ArgumentException("accountId is required", nameof(accountId));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("content is required", nameof(content));

        return new Comment
        {
            PostId = postId.Trim(),
            AccountId = accountId.Trim(),
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("content is required", nameof(content));
        Content = content.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}


