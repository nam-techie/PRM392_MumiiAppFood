using System.ComponentModel.DataAnnotations;

namespace Mumii.Shared.Common.DTOs;

/// <summary>
/// DTO cho mood/tâm trạng
/// </summary>
public record MoodDto(int Id, string Name, string? Description, DateTime CreatedAt);

/// <summary>
/// DTO cho bài đăng theo schema mới
/// </summary>
/// <summary>
/// DTO cho bài đăng theo schema mới
/// </summary>
public record PostDto(
    int Id,
    int PartnerId,
    int? RestaurantId,
    string Title,
    string Content,
    string? ImageUrl,

    // THÊM STATUS VÀO ĐÂY (hoặc một vị trí hợp lý khác)
    string Status,

    DateTime CreatedAt,
    List<MoodDto> Moods,
    RestaurantDto? Restaurant,
    UserDto? Partner
);

/// <summary>
/// DTO cho tạo bài đăng mới
/// </summary>
public record CreatePostRequest(string Title, string Content, string? ImageUrl, int? RestaurantId);

/// <summary>
/// DTO cho cập nhật bài đăng
/// </summary>
public record UpdatePostRequest(string Title, string Content, string? ImageUrl, int? RestaurantId);


/// <summary>
/// DTO cho tạo mood mới
/// </summary>
public record CreateMoodRequest(string Name, string? Description);

public record UpdateMoodRequest(string Name, string? Description);

// DTO này đã có: UpdatePostRequest
// Chúng ta sẽ tạo một DTO mới cho Admin
public record AdminUpdatePostRequest(string Title, string Content, string? ImageUrl, int? RestaurantId);

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

public record RestaurantSna(int Id, string Name, string Address);



public record PartnerDto(int Id, string Fullname, string Email, string? Avatar);

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


public record CommentDto(
    int Id,       // int
    int PostId,   // int
    int UserId,   // int
    string Content,
    DateTime CreatedAt,
    UserDto? User
);

/// <summary>
/// DTO để User gửi bình luận mới
/// </summary>
public record CreateCommentRequest(
    [Required(ErrorMessage = "Nội dung bình luận không được để trống.")]
    [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự.")]
    string Content
);