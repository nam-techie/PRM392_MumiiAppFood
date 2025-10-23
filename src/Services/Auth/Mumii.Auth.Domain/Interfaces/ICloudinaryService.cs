namespace Mumii.Auth.Domain.Interfaces;

/// <summary>
/// Interface cho Cloudinary service
/// </summary>
public interface ICloudinaryService
{
    /// <summary>
    /// Upload ảnh lên Cloudinary
    /// </summary>
    /// <param name="file">File ảnh</param>
    /// <param name="folder">Thư mục lưu trữ (mặc định: "avatars")</param>
    /// <returns>URL của ảnh đã upload</returns>
    Task<string> UploadImageAsync(IFileData file, string folder = "avatars");
}
