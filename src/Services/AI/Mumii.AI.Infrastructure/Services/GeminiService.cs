using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mumii.AI.Domain.Interfaces;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Mumii.AI.Infrastructure.Services;

public class GeminiService : IGeminiService
{
    private readonly string _apiKey;
    private readonly ILogger<GeminiService> _logger;
    private readonly HttpClient _http = new HttpClient();
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _logger = logger;

        _apiKey = configuration["GEMINI_API_KEY"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("⚠️ Gemini API key chưa được cấu hình. Vui lòng đặt biến môi trường GEMINI_API_KEY.");
        }

        _logger.LogInformation("Gemini Service initialized with a valid API Key.");
    }

	private static void TryLoadLocalEnv(ILogger logger)
	{
		try
		{
			var candidates = new List<string?>
			{
				Path.Combine(AppContext.BaseDirectory, ".env"),
				Path.Combine(Directory.GetCurrentDirectory(), ".env"),
				Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
				Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env")
			};

			var path = candidates.FirstOrDefault(p => p != null && File.Exists(p));
			if (path == null) return;

			var lines = File.ReadAllLines(path);
			int setCount = 0;
			foreach (var raw in lines)
			{
				var line = raw?.Trim();
				if (string.IsNullOrWhiteSpace(line)) continue;
				if (line.StartsWith("#")) continue;
				int idx = line.IndexOf('=');
				if (idx <= 0) continue;
				var key = line.Substring(0, idx).Trim();
				var value = line.Substring(idx + 1).Trim().Trim('"');
				if (string.IsNullOrWhiteSpace(key)) continue;
				Environment.SetEnvironmentVariable(key, value);
				setCount++;
			}

			if (setCount > 0)
			{
				logger.LogInformation("Loaded {Count} entries from .env", setCount);
			}
		}
		catch (Exception ex)
		{
			// Non-fatal for prod; only helps local dev.
			logger.LogDebug(ex, "Failed to load .env (safe to ignore in production)");
		}
	}

	private static string ComputeSha256Hex(string input)
	{
		using var sha = SHA256.Create();
		var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
		var sb = new StringBuilder(bytes.Length * 2);
		foreach (var b in bytes) sb.Append(b.ToString("x2"));
		return sb.ToString()[..16]; // short digest for logs
	}

    public async Task<string> ChatAboutFoodAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        try
        {
			var prompt = $@"Bạn là một AI chuyên gia ẩm thực Việt Nam. 
Trả lời thân thiện, chính xác câu hỏi: {userMessage}";
			var json = await GenerateContentAsync(prompt, cancellationToken);
			return ExtractText(json) ?? "Xin lỗi, tôi không thể trả lời lúc này.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini");
            return "Xin lỗi, đã có lỗi xảy ra.";
        }
    }

    public async Task<string> SuggestFoodByMoodAsync(string mood, string? location = null, CancellationToken cancellationToken = default)
    {
        try
        {
			var locationText = !string.IsNullOrWhiteSpace(location) ? $" tại {location}" : string.Empty;

			var prompt = $@"Bạn là AI ẩm thực Việt Nam. Dựa trên tâm trạng ""{mood}""{locationText}, gợi ý 3–5 món ăn.
- Trả lời bằng tiếng Việt
- Nêu lý do phù hợp với tâm trạng
- Nếu có địa điểm, ưu tiên món địa phương";

			var json = await GenerateContentAsync(prompt, cancellationToken);
			return ExtractText(json) ?? "Xin lỗi, tôi không thể gợi ý lúc này.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SuggestFoodByMoodAsync error");
            return "Xin lỗi, đã có lỗi xảy ra.";
        }
    }

    public async Task<string> AnalyzeFoodImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        try
        {
			var prompt = $@"Hãy phân tích nội dung của hình ảnh đồ ăn tại URL sau: {imageUrl}

Yêu cầu trả lời bằng tiếng Việt theo các mục:
1. Tên món (nếu nhận diện)
2. Loại món
3. Thành phần chính
4. Cách chế biến (suy đoán)
5. Độ hấp dẫn/chất lượng
6. Gợi ý món tương tự.";

			var json = await GenerateContentAsync(prompt, cancellationToken);
			return ExtractText(json) ?? "Xin lỗi, tôi không thể phân tích hình ảnh này.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnalyzeFoodImageAsync error");
            return "Xin lỗi, đã có lỗi xảy ra.";
        }
    }

    public async Task<string> SuggestRestaurantsAsync(string preferences, string? location = null, CancellationToken cancellationToken = default)
    {
        try
        {
			var locationText = !string.IsNullOrWhiteSpace(location) ? $" tại {location}" : string.Empty;

			var prompt = $@"Bạn là AI ẩm thực Việt Nam. Dựa trên sở thích và yêu cầu sau{locationText}, gợi ý 3–5 nhà hàng phù hợp.
Sở thích/Yêu cầu: {preferences}
- Trả lời bằng tiếng Việt
- Nêu lý do chọn nhà hàng, món nổi bật, khoảng giá";

			var json = await GenerateContentAsync(prompt, cancellationToken);
			return ExtractText(json) ?? "Xin lỗi, tôi không thể gợi ý nhà hàng lúc này.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SuggestRestaurantsAsync error");
            return "Xin lỗi, đã có lỗi xảy ra.";
        }
    }

	private async Task<string> GenerateContentAsync(string prompt, CancellationToken cancellationToken)
	{
		var url = $"{BaseUrl}/gemini-2.0-flash:generateContent?key={_apiKey}";

		var body = new
		{
			contents = new[]
			{
				new
				{
					parts = new object[]
					{
						new { text = prompt }
					}
				}
			}
		};

		var json = JsonSerializer.Serialize(body);
		using var content = new StringContent(json, Encoding.UTF8, "application/json");
		using var response = await _http.PostAsync(url, content, cancellationToken);
		var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError("Gemini REST call failed: {Status} | {Body}", (int)response.StatusCode, responseText);
		}

		return responseText;
	}

	private static string? ExtractText(string json)
	{
		try
		{
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;
			if (root.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array && candidates.GetArrayLength() > 0)
			{
				var first = candidates[0];
				if (first.TryGetProperty("content", out var content) && content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array && parts.GetArrayLength() > 0)
				{
					var part = parts[0];
					if (part.TryGetProperty("text", out var text))
					{
						return text.GetString();
					}
				}
			}
		}
		catch
		{
			// ignore parse errors; caller will fallback to default message
		}
		return null;
	}
}
