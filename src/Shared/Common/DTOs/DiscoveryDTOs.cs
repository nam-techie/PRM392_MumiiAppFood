using System.ComponentModel.DataAnnotations;

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
    string? Comment,
    DateTime CreatedAt,
    UserDto? User,
    string? PartnerReplyComment,
    DateTime? PartnerReplyAt
);

/// <summary>
/// DTO cho nhà hàng trả lời review
/// </summary>
public record ReplyToReviewRequest(
    [Required]
    [StringLength(500, ErrorMessage = "Phản hồi không được vượt quá 500 ký tự.")]
    string Comment
);

/// <summary>
/// DTO cho favorite nhà hàng
/// </summary>
public record FavoriteDto(
    int Id,
    int UserId,
    int RestaurantId,
    DateTime CreatedAt,
    RestaurantDto? Restaurant // Sửa thành nullable
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
    double? Latitude,
    double? Longitude,
    double? RadiusKm,
    double? MinPrice,
    double? MaxPrice,
    float? MinRating,
    string? Status, // <<< DI CHUYỂN LÊN TRƯỚC
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
    double Latitude,
    double Longitude,
    string? Status, // <<< DI CHUYỂN LÊN TRƯỚC
    double RadiusKm = 5.0,
    int Limit = 50
);
