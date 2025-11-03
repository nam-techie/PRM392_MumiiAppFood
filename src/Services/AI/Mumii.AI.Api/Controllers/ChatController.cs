using Microsoft.AspNetCore.Mvc;
using Mumii.AI.Domain.Interfaces;
using Mumii.Shared.Common.Models;

namespace Mumii.AI.Api.Controllers;

/// <summary>
/// Controller cho AI Chat với Gemini
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IGeminiService geminiService, ILogger<ChatController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Chat với AI về đồ ăn
    /// </summary>
    [HttpPost("food")]
    public async Task<ActionResult<ApiResponse<System.Text.Json.JsonElement>>> ChatAboutFood(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ApiResponse<System.Text.Json.JsonElement>.ErrorResult(
                    "Dữ liệu không hợp lệ",
                    "Tin nhắn không được để trống"));
            }

            var response = await _geminiService.ChatAboutFoodAsync(request.Message, cancellationToken);
            
            _logger.LogInformation("AI chat completed for message: {Message}", request.Message);
            return Ok(ApiResponse<System.Text.Json.JsonElement>.SuccessResult(response, "Chat thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI chat");
            return StatusCode(500, ApiResponse<System.Text.Json.JsonElement>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi chat với AI"));
        }
    }

    /// <summary>
    /// Gợi ý món ăn theo mood
    /// </summary>
    [HttpPost("suggest-by-mood")]
    public async Task<ActionResult<ApiResponse<System.Text.Json.JsonElement>>> SuggestByMood(
        [FromBody] MoodSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Mood))
            {
                return BadRequest(ApiResponse<System.Text.Json.JsonElement>.ErrorResult(
                    "Dữ liệu không hợp lệ",
                    "Mood không được để trống"));
            }

            var response = await _geminiService.SuggestFoodByMoodAsync(
                request.Mood, 
                request.Location, 
                cancellationToken);
            
            _logger.LogInformation("Mood suggestion completed for: {Mood}", request.Mood);
            return Ok(ApiResponse<System.Text.Json.JsonElement>.SuccessResult(response, "Gợi ý thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mood suggestion");
            return StatusCode(500, ApiResponse<System.Text.Json.JsonElement>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi gợi ý món ăn"));
        }
    }

    /// <summary>
    /// Gợi ý nhà hàng
    /// </summary>
    [HttpPost("suggest-restaurants")]
    public async Task<ActionResult<ApiResponse<System.Text.Json.JsonElement>>> SuggestRestaurants(
        [FromBody] RestaurantSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Preferences))
            {
                return BadRequest(ApiResponse<System.Text.Json.JsonElement>.ErrorResult(
                    "Dữ liệu không hợp lệ",
                    "Preferences không được để trống"));
            }

            var response = await _geminiService.SuggestRestaurantsAsync(
                request.Preferences, 
                request.Location, 
                cancellationToken);
            
            _logger.LogInformation("Restaurant suggestion completed");
            return Ok(ApiResponse<System.Text.Json.JsonElement>.SuccessResult(response, "Gợi ý nhà hàng thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in restaurant suggestion");
            return StatusCode(500, ApiResponse<System.Text.Json.JsonElement>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi gợi ý nhà hàng"));
        }
    }
}

/// <summary>
/// Request cho chat
/// </summary>
public record ChatRequest(string Message);

/// <summary>
/// Request cho gợi ý theo mood
/// </summary>
public record MoodSuggestionRequest(string Mood, string? Location = null);

/// <summary>
/// Request cho gợi ý nhà hàng
/// </summary>
public record RestaurantSuggestionRequest(string Preferences, string? Location = null);
