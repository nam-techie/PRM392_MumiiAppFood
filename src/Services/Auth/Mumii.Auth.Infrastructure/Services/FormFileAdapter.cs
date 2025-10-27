using Microsoft.AspNetCore.Http;
using Mumii.Auth.Domain.Interfaces;

namespace Mumii.Auth.Infrastructure.Services;

/// <summary>
/// Adapter để chuyển đổi IFormFile thành IFileData
/// Giúp Infrastructure layer làm việc với ASP.NET Core mà không vi phạm Clean Architecture
/// </summary>
public sealed class FormFileAdapter : IFileData
{
    private readonly IFormFile _file;

    /// <summary>
    /// Khởi tạo adapter với IFormFile
    /// </summary>
    /// <param name="file">File từ ASP.NET Core</param>
    public FormFileAdapter(IFormFile file)
    {
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }

    /// <summary>
    /// Mở stream để đọc file
    /// </summary>
    /// <returns>Stream để đọc file</returns>
    public Stream OpenRead() => _file.OpenReadStream();

    /// <summary>
    /// Tên file
    /// </summary>
    public string FileName => _file.FileName;

    /// <summary>
    /// Loại MIME của file
    /// </summary>
    public string ContentType => _file.ContentType;

    /// <summary>
    /// Kích thước file (bytes)
    /// </summary>
    public long Length => _file.Length;
}
