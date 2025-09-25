namespace Mumii.Social.Domain.Entities;

/// <summary>
/// Entity reaction
/// </summary>
public class Reaction
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string PostId { get; private set; } = string.Empty;
    public string AccountId { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation properties
    public Post Post { get; private set; } = null!;

    /// <summary>
    /// Constructor cho Entity Framework
    /// </summary>
    private Reaction() { }

    /// <summary>
    /// Tạo reaction mới
    /// </summary>
    public static Reaction Create(
        string postId,
        string accountId,
        string type)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(postId))
            throw new ArgumentException("Post ID không được để trống", nameof(postId));
        
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID không được để trống", nameof(accountId));
        
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Loại reaction không được để trống", nameof(type));

        // Validate reaction type
        var validTypes = new[] { "LIKE", "LOVE", "WOW" };
        if (!validTypes.Contains(type.ToUpper()))
            throw new ArgumentException($"Loại reaction không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validTypes)}", nameof(type));

        return new Reaction
        {
            PostId = postId,
            AccountId = accountId,
            Type = type.ToUpper(),
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cập nhật loại reaction
    /// </summary>
    public void UpdateType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Loại reaction không được để trống", nameof(type));

        // Validate reaction type
        var validTypes = new[] { "LIKE", "LOVE", "WOW" };
        if (!validTypes.Contains(type.ToUpper()))
            throw new ArgumentException($"Loại reaction không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validTypes)}", nameof(type));

        Type = type.ToUpper();
    }
}
