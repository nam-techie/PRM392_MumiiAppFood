namespace Mumii.Social.Domain.Entities;

/// <summary>
/// Entity mood/tâm trạng
/// </summary>
public class Mood
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation properties
    public List<PostMood> PostMoods { get; private set; } = new();

    /// <summary>
    /// Constructor cho Entity Framework
    /// </summary>
    private Mood() { }

    /// <summary>
    /// Tạo mood mới
    /// </summary>
    public static Mood Create(string name, string? description = null)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên mood không được để trống", nameof(name));

        if (name.Length > 50)
            throw new ArgumentException("Tên mood không được vượt quá 50 ký tự", nameof(name));

        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
            throw new ArgumentException("Mô tả không được vượt quá 500 ký tự", nameof(description));

        return new Mood
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cập nhật mood
    /// </summary>
    public void Update(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên mood không được để trống", nameof(name));

        if (name.Length > 50)
            throw new ArgumentException("Tên mood không được vượt quá 50 ký tự", nameof(name));

        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
            throw new ArgumentException("Mô tả không được vượt quá 500 ký tự", nameof(description));

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
