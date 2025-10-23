using Mumii.Shared.Common.Constants;

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
    public string Status { get; private set; } = RestaurantStatus.Pending;
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
        float rating = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên nhà hàng không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Địa chỉ không được để trống", nameof(address));
        
        // PartnerId là bắt buộc khi tạo
        if (partnerId <= 0)
            throw new ArgumentException("Cần có thông tin Partner ID hợp lệ.", nameof(partnerId));

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
            Status = RestaurantStatus.Pending, // <-- MẶC ĐỊNH LÀ PENDING
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    // Phương thức Update cho Partner (chỉ được sửa khi đang Pending hoặc Approved)
    public void UpdateByPartner(
        string name,
        string address,
        string? description = null,
        double? avgPrice = null)
    {
        if (Status == RestaurantStatus.Declined)
            throw new InvalidOperationException("Không thể cập nhật nhà hàng đã bị từ chối.");
        
         if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên nhà hàng không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Địa chỉ không được để trống", nameof(address));
        
        Name = name.Trim();
        Address = address.Trim();
        Description = description?.Trim();
        AvgPrice = avgPrice;
        // Nếu Partner sửa thông tin, có thể cần Admin duyệt lại
        // Status = RestaurantStatus.Pending; 
        UpdatedAt = DateTime.UtcNow;
    }

    // Phương thức cho Admin duyệt
    public void Approve()
    {
        if (Status != RestaurantStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể duyệt nhà hàng đang ở trạng thái chờ.");
        
        Status = RestaurantStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    // Phương thức cho Admin từ chối
    public void Decline()
    {
        if (Status != RestaurantStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể từ chối nhà hàng đang ở trạng thái chờ.");
        
        Status = RestaurantStatus.Declined;
        UpdatedAt = DateTime.UtcNow;
    }
}
