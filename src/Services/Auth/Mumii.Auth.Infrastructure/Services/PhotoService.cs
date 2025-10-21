using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Settings;
using System.IO;

namespace Mumii.Auth.Infrastructure.Services;
public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    public PhotoService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<(string? Url, string? PublicId)> AddPhotoAsync(Stream fileStream, string fileName)
    {
        if (fileStream == null || fileStream.Length == 0) return (null, null);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
            Folder = "mumii_avatars"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception(uploadResult.Error.Message);
        }

        return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
    }

    public async Task<bool> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }
}
