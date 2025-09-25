namespace Mumii.AI.Domain.Interfaces;

/// <summary>
/// Interface cho Gemini AI service
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// Chat với AI về đồ ăn
    /// </summary>
    Task<string> ChatAboutFoodAsync(string userMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gợi ý món ăn dựa trên mood
    /// </summary>
    Task<string> SuggestFoodByMoodAsync(string mood, string? location = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Phân tích hình ảnh đồ ăn
    /// </summary>
    Task<string> AnalyzeFoodImageAsync(string imageUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gợi ý nhà hàng dựa trên preferences
    /// </summary>
    Task<string> SuggestRestaurantsAsync(string preferences, string? location = null, CancellationToken cancellationToken = default);
}
