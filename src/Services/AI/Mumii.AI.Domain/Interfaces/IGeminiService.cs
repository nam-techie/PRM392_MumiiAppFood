namespace Mumii.AI.Domain.Interfaces;

/// <summary>
/// Interface cho Gemini AI service
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// Chat với AI về đồ ăn
    /// </summary>
    Task<System.Text.Json.JsonElement> ChatAboutFoodAsync(string userMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gợi ý món ăn dựa trên mood
    /// </summary>
    Task<System.Text.Json.JsonElement> SuggestFoodByMoodAsync(string mood, string? location = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gợi ý nhà hàng dựa trên preferences
    /// </summary>
    Task<System.Text.Json.JsonElement> SuggestRestaurantsAsync(string preferences, string? location = null, CancellationToken cancellationToken = default);
}
