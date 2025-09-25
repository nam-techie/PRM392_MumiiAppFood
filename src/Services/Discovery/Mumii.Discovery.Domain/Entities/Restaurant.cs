using Mumii.Shared.Common.Events;

namespace Mumii.Discovery.Domain.Entities;

/// <summary>
/// Entity nhà hàng
/// </summary>
public class Restaurant
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string? Region { get; private set; }
    public decimal? AvgPrice { get; private set; }
    public decimal Rating { get; private set; } = 0;
    public string? Description { get; private set; }
    public List<string> ImageUrls { get; private set; } = new();
    public List<string> Tags { get; private set; } = new();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; } = false;

    // Domain events
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Constructor cho Entity Framework
    /// </summary>
    private Restaurant() { }

    /// <summary>
    /// Tạo nhà hàng mới
    /// </summary>
    public static Restaurant Create(
        string name,
        string address,
        decimal? latitude = null,
        decimal? longitude = null,
        string? region = null,
        decimal? avgPrice = null,
        string? description = null,
        List<string>? imageUrls = null,
        List<string>? tags = null)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên nhà hàng không được để trống", nameof(name));
        
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Địa chỉ không được để trống", nameof(address));

        // Validate coordinates if provided
        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            throw new ArgumentException("Latitude phải trong khoảng -90 đến 90", nameof(latitude));
            
        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            throw new ArgumentException("Longitude phải trong khoảng -180 đến 180", nameof(longitude));

        // Validate price
        if (avgPrice.HasValue && avgPrice < 0)
            throw new ArgumentException("Giá trung bình không được âm", nameof(avgPrice));

        var restaurant = new Restaurant
        {
            Name = name.Trim(),
            Address = address.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            Region = region?.Trim(),
            AvgPrice = avgPrice,
            Description = description?.Trim(),
            ImageUrls = imageUrls ?? new List<string>(),
            Tags = tags ?? new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add domain event
        restaurant._domainEvents.Add(new RestaurantCreatedEvent(
            restaurant.Id,
            restaurant.Name,
            restaurant.Address,
            restaurant.Latitude,
            restaurant.Longitude
        ));

        return restaurant;
    }

    /// <summary>
    /// Cập nhật thông tin nhà hàng
    /// </summary>
    public void Update(
        string name,
        string address,
        decimal? latitude = null,
        decimal? longitude = null,
        string? region = null,
        decimal? avgPrice = null,
        string? description = null,
        List<string>? imageUrls = null,
        List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên nhà hàng không được để trống", nameof(name));
        
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Địa chỉ không được để trống", nameof(address));

        Name = name.Trim();
        Address = address.Trim();
        Latitude = latitude;
        Longitude = longitude;
        Region = region?.Trim();
        AvgPrice = avgPrice;
        Description = description?.Trim();
        ImageUrls = imageUrls ?? ImageUrls;
        Tags = tags ?? Tags;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cập nhật rating
    /// </summary>
    public void UpdateRating(decimal newRating, int totalRatings)
    {
        if (newRating < 0 || newRating > 5)
            throw new ArgumentException("Rating phải trong khoảng 0-5", nameof(newRating));

        Rating = newRating;
        UpdatedAt = DateTime.UtcNow;

        // Add domain event
        _domainEvents.Add(new RestaurantRatingUpdatedEvent(
            Id,
            newRating,
            totalRatings
        ));
    }

    /// <summary>
    /// Soft delete nhà hàng
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Khôi phục nhà hàng đã xóa
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Tính khoảng cách đến vị trí khác (km)
    /// </summary>
    public double? CalculateDistanceTo(decimal lat, decimal lng)
    {
        if (!Latitude.HasValue || !Longitude.HasValue)
            return null;

        return CalculateHaversineDistance(
            (double)Latitude.Value,
            (double)Longitude.Value,
            (double)lat,
            (double)lng
        );
    }

    /// <summary>
    /// Clear domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Tính khoảng cách Haversine
    /// </summary>
    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Bán kính Trái Đất (km)

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
