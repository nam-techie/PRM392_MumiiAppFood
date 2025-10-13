namespace Mumii.Social.Domain.Entities;

/// <summary>
/// Entity bài đăng theo schema mới (Mongo)
/// </summary>
public class Post
{
    public int Id { get; private set; }
    public int PartnerId { get; private set; }
    public int? RestaurantId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public List<PostMood> PostMoods { get; private set; } = new();

    private Post() { }

    public static Post Create(int id, int partnerId, string title, string content, string? imageUrl = null, int? restaurantId = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tiêu đề không được để trống", nameof(title));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Nội dung không được để trống", nameof(content));

        return new Post
        {
            Id = id,
            PartnerId = partnerId,
            Title = title.Trim(),
            Content = content.Trim(),
            ImageUrl = imageUrl?.Trim(),
            RestaurantId = restaurantId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string content, string? imageUrl = null, int? restaurantId = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tiêu đề không được để trống", nameof(title));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Nội dung không được để trống", nameof(content));

        Title = title.Trim();
        Content = content.Trim();
        ImageUrl = imageUrl?.Trim();
        RestaurantId = restaurantId;
        UpdatedAt = DateTime.UtcNow;
    }
}
