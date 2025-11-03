using MongoDB.Bson.Serialization.Attributes;

namespace Mumii.Discovery.Domain.Entities;

public class RestaurantImage
{
    [BsonId] // Mỗi ảnh sẽ có ID riêng trong mảng
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; private set; } = string.Empty;

    [BsonElement("url")]
    public string ImageUrl { get; private set; } = string.Empty;
    
    [BsonElement("public_id")]
    public string PublicId { get; private set; } = string.Empty; // Để xóa trên Cloudinary

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private RestaurantImage() { }

    public static RestaurantImage Create(string imageUrl, string publicId)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("URL hình ảnh không được để trống.", nameof(imageUrl));
        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("Public ID không được để trống.", nameof(publicId));

        return new RestaurantImage
        {
            Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
            ImageUrl = imageUrl,
            PublicId = publicId
        };
    }
}
