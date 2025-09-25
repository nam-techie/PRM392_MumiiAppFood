namespace Mumii.Shared.Common.Enums;

/// <summary>
/// Enum cho vai trò người dùng
/// </summary>
public enum UserRole
{
    User = 0,
    Admin = 1
}

/// <summary>
/// Enum cho trạng thái tài khoản
/// </summary>
public enum AccountStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2,
    Deleted = 3
}

/// <summary>
/// Enum cho loại bài đăng
/// </summary>
public enum PostType
{
    General = 0,
    Review = 1,
    Recommendation = 2
}

/// <summary>
/// Enum cho trạng thái bài đăng
/// </summary>
public enum PostStatus
{
    Active = 0,
    Hidden = 1,
    Deleted = 2,
    Reported = 3
}

/// <summary>
/// Enum cho loại nhà hàng
/// </summary>
public enum RestaurantType
{
    Restaurant = 0,
    Cafe = 1,
    FastFood = 2,
    StreetFood = 3,
    Bar = 4,
    Bakery = 5,
    Other = 99
}

/// <summary>
/// Enum cho khoảng giá
/// </summary>
public enum PriceRange
{
    Budget = 0,      // Dưới 50k
    Moderate = 1,    // 50k - 200k  
    Expensive = 2,   // 200k - 500k
    Luxury = 3       // Trên 500k
}

/// <summary>
/// Enum cho vùng miền
/// </summary>
public enum Region
{
    HaNoi = 0,
    HoChiMinh = 1,
    DaNang = 2,
    CanTho = 3,
    HaiPhong = 4,
    Other = 99
}
