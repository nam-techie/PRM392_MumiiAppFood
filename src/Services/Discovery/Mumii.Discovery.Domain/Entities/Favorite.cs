namespace Mumii.Discovery.Domain.Entities;

/// <summary>
/// Entity nhà hàng yêu thích của người dùng
/// </summary>
public class Favorite
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int RestaurantId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Favorite() { }

    public static Favorite Create(int id, int userId, int restaurantId)
    {
        return new Favorite
        {
            Id = id,
            UserId = userId,
            RestaurantId = restaurantId,
            CreatedAt = DateTime.UtcNow
        };
    }
}


