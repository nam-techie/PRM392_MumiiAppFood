namespace Mumii.Shared.Common.DTOs;

/// <summary>
/// DTO cho thông tin nhà hàng theo schema mới
/// </summary>
public record RestaurantDto(
    int Id,
    int PartnerId,
    string Name,
    string Address,
    double? Longitude,
    double? Latitude,
    string? Description,
    double? AvgPrice,
    float Rating,
    string Status,
    DateTime CreatedAt,
    List<RestaurantImageDto> Images,
    List<ReviewDto> Reviews,
    int FavoriteCount
);

/// <summary>
/// DTO cho hình ảnh nhà hàng
/// </summary>
public record RestaurantImageDto(
    string Id, // Sửa thành string
    int RestaurantId,
    string ImageUrl,
    DateTime CreatedAt
);

/// <summary>
/// DTO cho review nhà hàng
/// </summary>
public record ReviewDto(
    int Id,
    int UserId,
    int RestaurantId,
    int Rating,
    string Comment,
    DateTime CreatedAt,
    UserDto? User
);

/// <summary>
/// DTO cho favorite nhà hàng
/// </summary>
public record FavoriteDto(
    int Id,
    int UserId,
    int RestaurantId,
    DateTime CreatedAt,
    RestaurantDto Restaurant
);

/// <summary>
/// DTO cho tạo nhà hàng mới
/// </summary>
public record CreateRestaurantRequest(
    string Name,
    string Address,
    double? Latitude,
    double? Longitude,
    string? Description,
    double? AvgPrice
);

/// <summary>
/// DTO cho cập nhật nhà hàng
/// </summary>
public record UpdateRestaurantRequest(
    string Name,
    string Address,
    double? Latitude,
    double? Longitude,
    string? Description,
    double? AvgPrice,
    float? Rating,
    string? Status
);

/// <summary>
/// DTO cho tạo review
/// </summary>
public record CreateReviewRequest(
    int Rating,
    string Comment
);

/// <summary>
/// DTO cho cập nhật review
/// </summary>
public record UpdateReviewRequest(
    int Rating,
    string Comment
);

/// <summary>
/// DTO cho thêm hình ảnh nhà hàng
/// </summary>
public record AddRestaurantImageRequest(
    string ImageUrl
);

/// </summary>
public record SearchRestaurantsQuery(
    string? Query,
    double? Latitude,     // <<< Sửa thành double?
    double? Longitude,    // <<< Sửa thành double?
    double? RadiusKm,     // <<< Sửa thành double?
    double? MinPrice,     // <<< Sửa thành double?
    double? MaxPrice,     // <<< Sửa thành double?
    float? MinRating,     // <<< Sửa thành float? để khớp với Rating
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
    double Latitude,      // <<< Sửa thành double
    double Longitude,     // <<< Sửa thành double
    double RadiusKm = 5.0, // <<< Sửa thành double
    int Limit = 50
);
