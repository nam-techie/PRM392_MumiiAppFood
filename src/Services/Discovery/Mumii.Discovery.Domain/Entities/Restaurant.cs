namespace Mumii.Discovery.Domain.Entities;

/// <summary>
/// Entity nhà hàng theo schema mục tiêu (MongoDB)
/// </summary>
public class Restaurant
{
    public int Id { get; private set; }
    public int PartnerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public double? Longitude { get; private set; }
    public double? Latitude { get; private set; }
    public string? Description { get; private set; }
    public double? AvgPrice { get; private set; }
    public float Rating { get; private set; }
    public string Status { get; private set; } = "Active";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private Restaurant() { }

    public static Restaurant Create(
        int id,
        int partnerId,
        string name,
        string address,
        double? latitude = null,
        double? longitude = null,
        string? description = null,
        double? avgPrice = null,
        float rating = 0,
        string status = "Active")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên nhà hàng không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Địa chỉ không được để trống", nameof(address));

        return new Restaurant
        {
            Id = id,
            PartnerId = partnerId,
            Name = name.Trim(),
            Address = address.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            Description = description?.Trim(),
            AvgPrice = avgPrice,
            Rating = rating,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string address,
        double? latitude = null,
        double? longitude = null,
        string? description = null,
        double? avgPrice = null,
        float? rating = null,
        string? status = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên nhà hàng không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Địa chỉ không được để trống", nameof(address));

        Name = name.Trim();
        Address = address.Trim();
        Latitude = latitude;
        Longitude = longitude;
        Description = description?.Trim();
        AvgPrice = avgPrice;
        if (rating.HasValue) Rating = rating.Value;
        if (!string.IsNullOrWhiteSpace(status)) Status = status!;
        UpdatedAt = DateTime.UtcNow;
    }
}
