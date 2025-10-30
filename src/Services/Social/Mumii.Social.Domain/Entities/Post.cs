using System;
using System.Collections.Generic;
using System.Linq;

namespace Mumii.Social.Domain.Entities;

public class Post
{
    public int Id { get; private set; }
    public int PartnerId { get; private set; }
    public int? RestaurantId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public string Status { get; private set; } = string.Empty;

    public List<PostMood> PostMoods { get; private set; } = new();

    private Post() { }

    public static Post Create(int id, int partnerId, string title, string content, string? imageUrl = null, int? restaurantId = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tiêu đề không được để trống", nameof(title));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Nội dung không được để trống", nameof(content));

        return new Post
        {
            Id = id,
            PartnerId = partnerId,
            Title = title.Trim(),
            Content = content.Trim(),
            ImageUrl = imageUrl?.Trim(),
            RestaurantId = restaurantId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string content, string? imageUrl = null, int? restaurantId = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tiêu đề không được để trống", nameof(title));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Nội dung không được để trống", nameof(content));

        Title = title.Trim();
        Content = content.Trim();
        ImageUrl = imageUrl?.Trim();
        RestaurantId = restaurantId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetImage(string? imageUrl)
    {
        ImageUrl = imageUrl?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (Status != "Pending")
            throw new InvalidOperationException("Chỉ có thể duyệt bài đăng đang ở trạng thái chờ.");
        Status = "Approved";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Decline()
    {
        if (Status != "Pending")
            throw new InvalidOperationException("Chỉ có thể từ chối bài đăng đang ở trạng thái chờ.");
        Status = "Declined";
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMood(int moodId)
    {
        if (!PostMoods.Any(pm => pm.MoodId == moodId))
        {
            PostMoods.Add(PostMood.Create(this.Id, moodId));
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveMood(int moodId)
    {
        var postMoodToRemove = PostMoods.FirstOrDefault(pm => pm.MoodId == moodId);
        if (postMoodToRemove != null)
        {
            PostMoods.Remove(postMoodToRemove);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
