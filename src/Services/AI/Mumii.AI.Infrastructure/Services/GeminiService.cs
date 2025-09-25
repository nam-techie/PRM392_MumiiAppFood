using GenerativeAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mumii.AI.Domain.Interfaces;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Mumii.AI.Infrastructure.Services;

public class GeminiService : IGeminiService
{
    private readonly string _apiKey;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _logger = logger;

		// Load .env locally (for development or when host env/config isn't wired)
		TryLoadLocalEnv(_logger);
			string? source = null;
			string? Read(string? value, string src)
			{
				if (!string.IsNullOrWhiteSpace(value)) { source = src; return value; }
				return null;
			}

			_apiKey =
				Read(configuration["Gemini:ApiKey"], "config:Gemini:ApiKey")
				?? Read(configuration["GEMINI_API_KEY"], "config:GEMINI_API_KEY")
				?? Read(Environment.GetEnvironmentVariable("Gemini__ApiKey"), "env:Gemini__ApiKey")
				?? Read(Environment.GetEnvironmentVariable("GEMINI_API_KEY"), "env:GEMINI_API_KEY")
				?? string.Empty;

        _apiKey = _apiKey?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
				throw new InvalidOperationException("⚠️ Gemini API key chưa được cấu hình (Gemini:ApiKey, Gemini__ApiKey hoặc GEMINI_API_KEY)");
        }

			// Handle values wrapped like ${AIza...}. If the inner part looks like a real key, accept it; otherwise fail.
			if (_apiKey.StartsWith("${") && _apiKey.EndsWith("}"))
			{
				var inner = _apiKey.Substring(2, _apiKey.Length - 3).Trim();
				if (!string.IsNullOrEmpty(inner) && inner.StartsWith("AIza"))
				{
					_logger.LogWarning("API key was wrapped in ${...}. Auto-unwrapped for use. Please remove ${} in configuration.");
					_apiKey = inner;
				}
				else
				{
					throw new InvalidOperationException("API key không hợp lệ: phát hiện placeholder ${...}. Hãy đặt giá trị key thật (ví dụ AiZa...).");
				}
			}

			var masked = _apiKey.Length >= 10 ? $"{_apiKey[..6]}...{_apiKey[^4..]}" : "(short/invalid)";
			string digest = ComputeSha256Hex(_apiKey);
			_logger.LogInformation("Gemini API key detected from {Source} (masked): {Key} | len={Len} | sha256={Sha}", source ?? "unknown", masked, _apiKey.Length, digest);
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
			var model = new GenerativeModel("gemini-2.0-flash", _apiKey);

            var resp = await model.GenerateContentAsync(
                $@"Bạn là một AI chuyên gia ẩm thực Việt Nam. 
Trả lời thân thiện, chính xác câu hỏi: {userMessage}",
                cancellationToken: cancellationToken);

            return resp?.Text ?? "Xin lỗi, tôi không thể trả lời lúc này.";
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
			var model = new GenerativeModel("gemini-2.0-flash", _apiKey);
            var locationText = !string.IsNullOrWhiteSpace(location) ? $" tại {location}" : string.Empty;

            var prompt = $@"Bạn là AI ẩm thực Việt Nam. Dựa trên tâm trạng ""{mood}""{locationText}, gợi ý 3–5 món ăn.
- Trả lời bằng tiếng Việt
- Nêu lý do phù hợp với tâm trạng
- Nếu có địa điểm, ưu tiên món địa phương";

            var resp = await model.GenerateContentAsync(prompt, cancellationToken: cancellationToken);
            return resp?.Text ?? "Xin lỗi, tôi không thể gợi ý lúc này.";
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
			var model = new GenerativeModel("gemini-2.0-flash", _apiKey);
            var prompt = $@"Hãy phân tích nội dung của hình ảnh đồ ăn tại URL sau: {imageUrl}

Yêu cầu trả lời bằng tiếng Việt theo các mục:
1. Tên món (nếu nhận diện)
2. Loại món
3. Thành phần chính
4. Cách chế biến (suy đoán)
5. Độ hấp dẫn/chất lượng
6. Gợi ý món tương tự.";

            var resp = await model.GenerateContentAsync(prompt, cancellationToken: cancellationToken);
            return resp?.Text ?? "Xin lỗi, tôi không thể phân tích hình ảnh này.";
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
			var model = new GenerativeModel("gemini-2.0-flash", _apiKey);
            var locationText = !string.IsNullOrWhiteSpace(location) ? $" tại {location}" : string.Empty;

            var prompt = $@"Bạn là AI ẩm thực Việt Nam. Dựa trên sở thích và yêu cầu sau{locationText}, gợi ý 3–5 nhà hàng phù hợp.
Sở thích/Yêu cầu: {preferences}
- Trả lời bằng tiếng Việt
- Nêu lý do chọn nhà hàng, món nổi bật, khoảng giá";

            var resp = await model.GenerateContentAsync(prompt, cancellationToken: cancellationToken);
            return resp?.Text ?? "Xin lỗi, tôi không thể gợi ý nhà hàng lúc này.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SuggestRestaurantsAsync error");
            return "Xin lỗi, đã có lỗi xảy ra.";
        }
    }
}
