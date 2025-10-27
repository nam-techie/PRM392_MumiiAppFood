namespace Mumii.Auth.Domain.Interfaces;

/// <summary>
/// Interface đại diện cho dữ liệu file thuần .NET
/// Không phụ thuộc vào ASP.NET Core để tuân thủ Clean Architecture
/// </summary>
public interface IFileData
{
    /// <summary>
    /// Mở stream để đọc file
    /// </summary>
    /// <returns>Stream để đọc file</returns>
    Stream OpenRead();
    
    /// <summary>
    /// Tên file
    /// </summary>
    string FileName { get; }
    
    /// <summary>
    /// Loại MIME của file
    /// </summary>
    string ContentType { get; }
    
    /// <summary>
    /// Kích thước file (bytes)
    /// </summary>
    long Length { get; }
}
