namespace Mumii.Shared.Common.Models;

/// <summary>
/// Chuẩn response cho tất cả API endpoints
/// </summary>
/// <typeparam name="T">Loại dữ liệu trả về</typeparam>
public record ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public List<string> Errors { get; init; } = new();
    public string? TraceId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Tạo response thành công
    /// </summary>
    public static ApiResponse<T> SuccessResult(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Tạo response thành công không có data
    /// </summary>
    public static ApiResponse<T> SuccessResult(string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Tạo response lỗi
    /// </summary>
    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    /// <summary>
    /// Tạo response lỗi với một lỗi duy nhất
    /// </summary>
    public static ApiResponse<T> ErrorResult(string message, string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { error }
        };
    }
}

/// <summary>
/// Response không có generic type
/// </summary>
public record ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Tạo response thành công không có data
    /// </summary>
    public static new ApiResponse SuccessResult(string message = "Success")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Tạo response lỗi
    /// </summary>
    public static new ApiResponse ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    /// <summary>
    /// Tạo response lỗi với một lỗi duy nhất
    /// </summary>
    public static new ApiResponse ErrorResult(string message, string error)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = new List<string> { error }
        };
    }
}
