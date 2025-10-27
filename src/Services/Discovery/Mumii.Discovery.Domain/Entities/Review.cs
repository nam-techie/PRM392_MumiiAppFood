using MongoDB.Bson.Serialization.Attributes; // Thêm using này nếu dùng MongoDB
using System;

namespace Mumii.Discovery.Domain.Entities;

/// <summary>
/// Entity đánh giá nhà hàng
/// </summary>
public class Review
{
    [BsonId] // Giả sử dùng MongoDB, nếu không thì bỏ
    [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]
    public int Id { get; private set; }
    
    public int UserId { get; private set; }
    public int RestaurantId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    [BsonElement("partner_reply")]
    [BsonIgnoreIfNull]
    public string? PartnerReplyComment { get; private set; }

    [BsonElement("partner_reply_at")]
    [BsonIgnoreIfNull]
    public DateTime? PartnerReplyAt { get; private set; }

    private Review() { }

    public static Review Create(int id, int userId, int restaurantId, int rating, string? comment = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating phải 1-5", nameof(rating));

        return new Review
        {
            Id = id,
            UserId = userId,
            RestaurantId = restaurantId,
            Rating = rating,
            Comment = comment?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(int rating, string? comment = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating phải 1-5", nameof(rating));

        Rating = rating;
        Comment = comment?.Trim();
    }

    public void AddOrUpdateReply(string replyComment)
    {
        if (string.IsNullOrWhiteSpace(replyComment))
        {
            throw new ArgumentException("Nội dung phản hồi không được để trống.", nameof(replyComment));
        }
        if (replyComment.Length > 500)
        {
            throw new ArgumentException("Phản hồi không được vượt quá 500 ký tự.", nameof(replyComment));
        }

        PartnerReplyComment = replyComment.Trim();
        PartnerReplyAt = DateTime.UtcNow;
    }

    public void RemoveReply()
    {
        PartnerReplyComment = null;
        PartnerReplyAt = null;
    }
}