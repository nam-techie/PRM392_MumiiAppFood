namespace Mumii.Social.Domain.Entities;

/// <summary>
/// Entity liên kết giữa Post và Mood (many-to-many)
/// </summary>
public class PostMood
{
    public int PostId { get; private set; }
    public int MoodId { get; private set; }

    // Navigation properties
    public Post Post { get; private set; } = null!;
    public Mood Mood { get; private set; } = null!;

    /// <summary>
    /// Constructor cho Entity Framework
    /// </summary>
    private PostMood() { }

    /// <summary>
    /// Tạo liên kết Post-Mood mới
    /// </summary>
    public static PostMood Create(int postId, int moodId)
    {
        return new PostMood
        {
            PostId = postId,
            MoodId = moodId
        };
    }
}
