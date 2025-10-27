using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Mumii.Social.Domain.Entities;

/// <summary>
/// Comment entity với ID kiểu int
/// </summary>
public class Comment
{
    [BsonId] // Vẫn là [BsonId] để MongoDB hiểu đây là khóa chính
    public int Id { get; private set; }

    public int PostId { get; private set; } // Sửa thành int
    public int UserId { get; private set; } // Đổi tên thành UserId và sửa thành int
    
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; } = false;

    private Comment() { }

    // Sửa lại phương thức Create để nhận vào int
    public static Comment Create(int id, int postId, int userId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) 
            throw new ArgumentException("Nội dung không được để trống", nameof(content));
        if (content.Length > 1000) 
            throw new ArgumentException("Bình luận không được vượt quá 1000 ký tự", nameof(content));

        return new Comment
        {
            Id = id,
            PostId = postId,
            UserId = userId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) 
            throw new ArgumentException("Nội dung không được để trống", nameof(content));
        Content = content.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
