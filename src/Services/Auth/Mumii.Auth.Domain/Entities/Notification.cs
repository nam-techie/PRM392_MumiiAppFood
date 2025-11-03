using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mumii.Auth.Domain.Entities;

/// <summary>
/// Entity thông báo cho người dùng
/// </summary>
public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.Int32)]
    public int Id { get; private set; }
    
    [BsonElement("user_id")]
    public int UserId { get; private set; }
    
    [BsonElement("title")]
    public string Title { get; private set; } = string.Empty;
    
    [BsonElement("content")]
    public string Content { get; private set; } = string.Empty;
    
    [BsonElement("is_read")]
    public bool IsRead { get; private set; } = false;
    
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation properties - Ignore in MongoDB
    [BsonIgnore]
    public User User { get; private set; } = null!;

    /// <summary>
    /// Constructor cho Entity Framework
    /// </summary>
    private Notification() { }

    /// <summary>
    /// Tạo notification mới
    /// </summary>
    public static Notification Create(int id, int userId, string title, string content)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề không được để trống", nameof(title));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung không được để trống", nameof(content));

        if (title.Length > 255)
            throw new ArgumentException("Tiêu đề không được vượt quá 255 ký tự", nameof(title));

        if (content.Length > 1000)
            throw new ArgumentException("Nội dung không được vượt quá 1000 ký tự", nameof(content));

        return new Notification
        {
            Id = id,
            UserId = userId,
            Title = title.Trim(),
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Đánh dấu đã đọc
    /// </summary>
    public void MarkAsRead()
    {
        IsRead = true;
    }

    /// <summary>
    /// Đánh dấu chưa đọc
    /// </summary>
    public void MarkAsUnread()
    {
        IsRead = false;
    }

    /// <summary>
    /// Cập nhật nội dung notification
    /// </summary>
    public void Update(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề không được để trống", nameof(title));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung không được để trống", nameof(content));

        if (title.Length > 255)
            throw new ArgumentException("Tiêu đề không được vượt quá 255 ký tự", nameof(title));

        if (content.Length > 1000)
            throw new ArgumentException("Nội dung không được vượt quá 1000 ký tự", nameof(content));

        Title = title.Trim();
        Content = content.Trim();
    }
}
