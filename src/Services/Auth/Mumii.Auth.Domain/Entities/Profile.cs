using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mumii.Auth.Domain.Entities;

/// <summary>
/// Entity profile người dùng
/// </summary>
public class Profile
{
    [BsonId]
    [BsonRepresentation(BsonType.Int32)]
    public int Id { get; private set; }
    
    [BsonElement("user_id")]
    public int UserId { get; private set; }
    
    [BsonElement("gender")]
    [BsonIgnoreIfNull]
    public string? Gender { get; private set; }
    
    [BsonElement("avatar")]
    [BsonIgnoreIfNull]
    public string? Avatar { get; private set; }
    
    [BsonElement("phone_number")]
    [BsonIgnoreIfNull]
    public string? PhoneNumber { get; private set; }
    
    [BsonElement("address")]
    [BsonIgnoreIfNull]
    public string? Address { get; private set; }
    
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    
    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation properties - Ignore in MongoDB
    [BsonIgnore]
    public User User { get; private set; } = null!;

    /// <summary>
    /// Constructor cho Entity Framework
    /// </summary>
    private Profile() { }

    /// <summary>
    /// Tạo profile mới
    /// </summary>
    public static Profile Create(int id, int userId, string? gender = null, string? avatar = null, 
        string? phoneNumber = null, string? address = null)
    {
        // Validate phone number format if provided
        if (!string.IsNullOrWhiteSpace(phoneNumber) && !IsValidPhoneNumber(phoneNumber))
            throw new ArgumentException("Số điện thoại không đúng định dạng", nameof(phoneNumber));

        // Validate gender if provided
        if (!string.IsNullOrWhiteSpace(gender))
        {
            var validGenders = new[] { "Male", "Female", "Other" };
            if (!validGenders.Contains(gender))
                throw new ArgumentException($"Giới tính không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validGenders)}", nameof(gender));
        }

        return new Profile
        {
            Id = id,
            UserId = userId,
            Gender = gender?.Trim(),
            Avatar = avatar?.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            Address = address?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cập nhật thông tin profile (không bao gồm avatar)
    /// </summary>
    public void Update(string? gender = null, string? phoneNumber = null, string? address = null)
    {
        // Validate phone number format if provided
        if (!string.IsNullOrWhiteSpace(phoneNumber) && !IsValidPhoneNumber(phoneNumber))
            throw new ArgumentException("Số điện thoại không đúng định dạng", nameof(phoneNumber));

        // Validate gender if provided
        if (!string.IsNullOrWhiteSpace(gender))
        {
            var validGenders = new[] { "Male", "Female", "Other" };
            if (!validGenders.Contains(gender))
                throw new ArgumentException($"Giới tính không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validGenders)}", nameof(gender));
        }

        Gender = gender?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Address = address?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cập nhật avatar
    /// </summary>
    public void UpdateAvatar(string avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
            throw new ArgumentException("URL avatar không được để trống", nameof(avatarUrl));

        Avatar = avatarUrl.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validate phone number format (Vietnamese format)
    /// </summary>
    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            // Nếu không bắt buộc phải có SĐT, ta coi chuỗi rỗng là hợp lệ.
            return true; 
        }

        // Chỉ giữ lại các ký tự số
        var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
        
        // Kiểm tra xem có phải là số điện thoại Việt Nam không
        // Bắt đầu bằng 0 và có 10 chữ số
        if (cleanPhone.StartsWith("0") && cleanPhone.Length == 10)
        {
            return true;
        }

        // Nếu không khớp, trả về false
        return false;
    }
}
