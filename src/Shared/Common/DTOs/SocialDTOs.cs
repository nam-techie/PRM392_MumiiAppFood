namespace Mumii.Shared.Common.DTOs;

/// <summary>
/// DTO cho mood/tâm trạng
/// </summary>
public record MoodDto(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAt
);

/// <summary>
/// DTO cho bài đăng theo schema mới
/// </summary>
public record PostDto(
    int Id,
    int PartnerId,
    int? RestaurantId,
    string Title,
    string Content,
    DateTime CreatedAt,
    List<MoodDto> Moods,
    RestaurantDto? Restaurant,
    UserDto Partner
);

/// <summary>
/// DTO cho tạo bài đăng mới
/// </summary>
public record CreatePostRequest(
    string Title,
    string Content,
    int? RestaurantId,
    List<int> MoodIds
);

/// <summary>
/// DTO cho cập nhật bài đăng
/// </summary>
public record UpdatePostRequest(
    string Title,
    string Content,
    int? RestaurantId,
    List<int> MoodIds
);

/// <summary>
/// DTO cho tạo mood mới
/// </summary>
public record CreateMoodRequest(
    string Name,
    string? Description
);

/// <summary>
/// DTO cho tìm kiếm bài đăng
/// </summary>
public record SearchPostsQuery(
    List<int>? MoodIds,
    int? RestaurantId,
    int? PartnerId,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20
);

// Xóa comment và reaction DTOs vì đã chuyển sang Review trong Discovery service

/// <summary>
/// Enum cho các loại mood
/// </summary>
public static class PostMoods
{
    public const string Happy = "HAPPY";
    public const string Excited = "EXCITED";
    public const string Hungry = "HUNGRY";
    public const string Satisfied = "SATISFIED";
    public const string Disappointed = "DISAPPOINTED";
    public const string Curious = "CURIOUS";
    public const string Nostalgic = "NOSTALGIC";
    
    public static readonly List<string> All = new()
    {
        Happy, Excited, Hungry, Satisfied, Disappointed, Curious, Nostalgic
    };
}

/// <summary>
/// Enum cho các loại reaction
/// </summary>
public static class ReactionTypes
{
    public const string Like = "LIKE";
    public const string Love = "LOVE";
    public const string Wow = "WOW";
    
    public static readonly List<string> All = new()
    {
        Like, Love, Wow
    };
}
