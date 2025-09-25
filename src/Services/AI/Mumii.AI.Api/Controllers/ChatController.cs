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
    public async Task<ActionResult<ApiResponse<string>>> ChatAboutFood(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ApiResponse<string>.ErrorResult(
                    "Dữ liệu không hợp lệ",
                    "Tin nhắn không được để trống"));
            }

            var response = await _geminiService.ChatAboutFoodAsync(request.Message, cancellationToken);
            
            _logger.LogInformation("AI chat completed for message: {Message}", request.Message);
            return Ok(ApiResponse<string>.SuccessResult(response, "Chat thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI chat");
            return StatusCode(500, ApiResponse<string>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi chat với AI"));
        }
    }

    /// <summary>
    /// Gợi ý món ăn theo mood
    /// </summary>
    [HttpPost("suggest-by-mood")]
    public async Task<ActionResult<ApiResponse<string>>> SuggestByMood(
        [FromBody] MoodSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Mood))
            {
                return BadRequest(ApiResponse<string>.ErrorResult(
                    "Dữ liệu không hợp lệ",
                    "Mood không được để trống"));
            }

            var response = await _geminiService.SuggestFoodByMoodAsync(
                request.Mood, 
                request.Location, 
                cancellationToken);
            
            _logger.LogInformation("Mood suggestion completed for: {Mood}", request.Mood);
            return Ok(ApiResponse<string>.SuccessResult(response, "Gợi ý thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mood suggestion");
            return StatusCode(500, ApiResponse<string>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi gợi ý món ăn"));
        }
    }

    /// <summary>
    /// Phân tích hình ảnh đồ ăn
    /// </summary>
    [HttpPost("analyze-image")]
    public async Task<ActionResult<ApiResponse<string>>> AnalyzeImage(
        [FromBody] ImageAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                return BadRequest(ApiResponse<string>.ErrorResult(
                    "Dữ liệu không hợp lệ",
                    "URL hình ảnh không được để trống"));
            }

            var response = await _geminiService.AnalyzeFoodImageAsync(request.ImageUrl, cancellationToken);
            
            _logger.LogInformation("Image analysis completed for: {ImageUrl}", request.ImageUrl);
            return Ok(ApiResponse<string>.SuccessResult(response, "Phân tích hình ảnh thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in image analysis");
            return StatusCode(500, ApiResponse<string>.ErrorResult(
                "Lỗi hệ thống",
                "Đã xảy ra lỗi khi phân tích hình ảnh"));
        }
    }

    /// <summary>
    /// Gợi ý nhà hàng
    /// </summary>
    [HttpPost("suggest-restaurants")]
    public async Task<ActionResult<ApiResponse<string>>> SuggestRestaurants(
        [FromBody] RestaurantSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Preferences))
            {
                return BadRequest(ApiResponse<string>.ErrorResult(
                    "Dữ liệu không hợp lệ",
                    "Preferences không được để trống"));
            }

            var response = await _geminiService.SuggestRestaurantsAsync(
                request.Preferences, 
                request.Location, 
                cancellationToken);
            
            _logger.LogInformation("Restaurant suggestion completed");
            return Ok(ApiResponse<string>.SuccessResult(response, "Gợi ý nhà hàng thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in restaurant suggestion");
            return StatusCode(500, ApiResponse<string>.ErrorResult(
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
/// Request cho phân tích hình ảnh
/// </summary>
public record ImageAnalysisRequest(string ImageUrl);

/// <summary>
/// Request cho gợi ý nhà hàng
/// </summary>
public record RestaurantSuggestionRequest(string Preferences, string? Location = null);
