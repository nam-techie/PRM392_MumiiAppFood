namespace Mumii.Shared.Common.DTOs;

/// <summary>
/// DTO cho bài đăng
/// </summary>
public record PostDto(
    string Id,
    string AccountId,
    string Content,
    string? Mood,
    List<string> ImageUrls,
    string? RestaurantId,
    int ReactionCount,
    int CommentCount,
    DateTime CreatedAt,
    AccountDto Account,
    RestaurantDto? Restaurant,
    UserReactionDto? UserReaction
);

/// <summary>
/// DTO cho tạo bài đăng mới
/// </summary>
public record CreatePostRequest(
    string Content,
    string? Mood,
    List<string> ImageUrls,
    string? RestaurantId
);

/// <summary>
/// DTO cho cập nhật bài đăng
/// </summary>
public record UpdatePostRequest(
    string Content,
    string? Mood,
    List<string> ImageUrls,
    string? RestaurantId
);

/// <summary>
/// DTO cho tìm kiếm bài đăng
/// </summary>
public record SearchPostsQuery(
    string? Mood,
    string? RestaurantId,
    string? AccountId,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20
);

/// <summary>
/// DTO cho comment
/// </summary>
public record CommentDto(
    string Id,
    string PostId,
    string AccountId,
    string Content,
    string? ParentCommentId,
    DateTime CreatedAt,
    AccountDto Account,
    List<CommentDto> Replies
);

/// <summary>
/// DTO cho tạo comment mới
/// </summary>
public record CreateCommentRequest(
    string Content,
    string? ParentCommentId
);

/// <summary>
/// DTO cho reaction
/// </summary>
public record ReactionDto(
    string Id,
    string PostId,
    string AccountId,
    string Type,
    DateTime CreatedAt,
    AccountDto Account
);

/// <summary>
/// DTO cho reaction của user hiện tại
/// </summary>
public record UserReactionDto(
    string Id,
    string Type,
    DateTime CreatedAt
);

/// <summary>
/// DTO cho toggle reaction
/// </summary>
public record ToggleReactionRequest(
    string Type // LIKE, LOVE, WOW
);

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
