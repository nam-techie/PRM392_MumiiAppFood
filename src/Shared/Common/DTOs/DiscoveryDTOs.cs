namespace Mumii.Shared.Common.DTOs;

/// <summary>
/// DTO cho thông tin nhà hàng
/// </summary>
public record RestaurantDto(
    string Id,
    string Name,
    string Address,
    decimal? Latitude,
    decimal? Longitude,
    string? Region,
    decimal? AvgPrice,
    decimal Rating,
    string? Description,
    List<string> ImageUrls,
    List<string> Tags,
    DateTime CreatedAt
);

/// <summary>
/// DTO cho tạo nhà hàng mới
/// </summary>
public record CreateRestaurantRequest(
    string Name,
    string Address,
    decimal? Latitude,
    decimal? Longitude,
    string? Region,
    decimal? AvgPrice,
    string? Description,
    List<string> ImageUrls,
    List<string> Tags
);

/// <summary>
/// DTO cho cập nhật nhà hàng
/// </summary>
public record UpdateRestaurantRequest(
    string Name,
    string Address,
    decimal? Latitude,
    decimal? Longitude,
    string? Region,
    decimal? AvgPrice,
    string? Description,
    List<string> ImageUrls,
    List<string> Tags
);

/// <summary>
/// DTO cho tìm kiếm nhà hàng
/// </summary>
public record SearchRestaurantsQuery(
    string? Query,
    string? Region,
    decimal? Latitude,
    decimal? Longitude,
    decimal? RadiusKm,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? MinRating,
    List<string>? Tags,
    int Page = 1,
    int PageSize = 20
);

/// <summary>
/// DTO cho kết quả tìm kiếm có phân trang
/// </summary>
public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

/// <summary>
/// DTO cho nhà hàng gần vị trí
/// </summary>
public record NearbyRestaurantsQuery(
    decimal Latitude,
    decimal Longitude,
    decimal RadiusKm = 5.0m,
    int Limit = 50
);
