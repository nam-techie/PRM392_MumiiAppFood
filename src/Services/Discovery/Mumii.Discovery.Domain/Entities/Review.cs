namespace Mumii.Discovery.Domain.Entities;

/// <summary>
/// Entity đánh giá nhà hàng
/// </summary>
public class Review
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int RestaurantId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

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
}


