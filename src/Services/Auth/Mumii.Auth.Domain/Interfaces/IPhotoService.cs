using System.IO; // Sử dụng Stream thay vì IFormFile
using System.Threading.Tasks;

namespace Mumii.Auth.Domain.Interfaces;

public interface IPhotoService
{
    /// <summary>
    /// Thêm một ảnh mới và trả về URL cùng Public ID của nó.
    /// </summary>
    /// <param name="fileStream">Luồng dữ liệu của file.</param>
    /// <param name="fileName">Tên file gốc.</param>
    /// <returns>Một tuple chứa URL và Public ID của ảnh.</returns>
    Task<(string? Url, string? PublicId)> AddPhotoAsync(Stream fileStream, string fileName);

    /// <summary>
    /// Xóa một ảnh dựa trên Public ID.
    /// </summary>
    /// <param name="publicId">Public ID của ảnh trên dịch vụ lưu trữ.</param>
    /// <returns>True nếu xóa thành công.</returns>
    Task<bool> DeletePhotoAsync(string publicId);
}