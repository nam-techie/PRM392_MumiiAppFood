using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Mumii.Auth.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Mumii.Auth.Infrastructure.Services;

/// <summary>
/// Service upload ảnh lên Cloudinary
/// </summary>
public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
    {
        _logger = logger;
        
        var cloudName = configuration["CloudinarySettings:CloudName"];
        var apiKey = configuration["CloudinarySettings:ApiKey"];
        var apiSecret = configuration["CloudinarySettings:ApiSecret"];

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new InvalidOperationException("Cloudinary settings are not configured properly");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    /// <summary>
    /// Upload ảnh lên Cloudinary
    /// </summary>
    public async Task<string> UploadImageAsync(IFileData file, string folder = "avatars")
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File không được để trống", nameof(file));
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                throw new ArgumentException("Chỉ chấp nhận file ảnh (JPG, PNG, WebP)", nameof(file));
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("File không được vượt quá 5MB", nameof(file));
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var publicId = $"{folder}/{fileName}";

            // Upload parameters
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, file.OpenRead()),
                PublicId = publicId,
                Folder = folder,
                Transformation = new Transformation()
                    .Width(400)
                    .Height(400)
                    .Crop("fill")
                    .Gravity("face")
                    .Quality("auto")
            };

            // Upload to Cloudinary
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Upload failed: {uploadResult.Error?.Message}");
            }

            _logger.LogInformation("Image uploaded successfully to Cloudinary: {PublicId}", publicId);
            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to Cloudinary");
            throw;
        }
    }
}
