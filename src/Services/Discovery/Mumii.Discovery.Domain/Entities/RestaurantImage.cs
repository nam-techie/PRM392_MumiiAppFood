namespace Mumii.Discovery.Domain.Entities;

/// <summary>
/// Entity hình ảnh nhà hàng
/// </summary>
public class RestaurantImage
{
    public int Id { get; private set; }
    public int RestaurantId { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation properties
    public Restaurant Restaurant { get; private set; } = null!;

    /// <summary>
    /// Constructor cho Entity Framework
    /// </summary>
    private RestaurantImage() { }

    /// <summary>
    /// Tạo restaurant image mới
    /// </summary>
    public static RestaurantImage Create(int restaurantId, string imageUrl)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("URL hình ảnh không được để trống", nameof(imageUrl));

        // Validate URL format
        if (!IsValidUrl(imageUrl))
            throw new ArgumentException("URL hình ảnh không đúng định dạng", nameof(imageUrl));

        return new RestaurantImage
        {
            RestaurantId = restaurantId,
            ImageUrl = imageUrl.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cập nhật URL hình ảnh
    /// </summary>
    public void UpdateImageUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("URL hình ảnh không được để trống", nameof(imageUrl));

        // Validate URL format
        if (!IsValidUrl(imageUrl))
            throw new ArgumentException("URL hình ảnh không đúng định dạng", nameof(imageUrl));

        ImageUrl = imageUrl.Trim();
    }

    /// <summary>
    /// Validate URL format
    /// </summary>
    private static bool IsValidUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            return uri.IsAbsoluteUri && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
        catch
        {
            return false;
        }
    }
}
