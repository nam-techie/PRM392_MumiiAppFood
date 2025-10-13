using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Mumii.Shared.Common.Enums;
using Mumii.Shared.Common.Events;

namespace Mumii.Auth.Domain.Entities;

/// <summary>
/// Entity người dùng theo schema mới
/// </summary>
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.Int32)]
    public int Id { get; private set; }
    
    [BsonElement("email")]
    public string Email { get; private set; } = string.Empty;
    
    [BsonElement("password")]
    public string Password { get; private set; } = string.Empty;
    
    [BsonElement("fullname")]
    public string Fullname { get; private set; } = string.Empty;
    
    [BsonElement("role")]
    public string Role { get; private set; } = "User";
    
    [BsonElement("is_active")]
    public bool IsActive { get; private set; } = true;
    
    [BsonElement("login_method")]
    public string LoginMethod { get; private set; } = "Email";
    
    [BsonElement("google_id")]
    [BsonIgnoreIfNull]
    public string? GoogleId { get; private set; }
    
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    
    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation properties - Ignore in MongoDB
    [BsonIgnore]
    public Profile? Profile { get; private set; }
    
    [BsonIgnore]
    public List<Notification> Notifications { get; private set; } = new();

    // Domain events - Ignore in MongoDB
    [BsonIgnore]
    private readonly List<IDomainEvent> _domainEvents = new();
    
    [BsonIgnore]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Constructor cho Entity Framework
    /// </summary>
    private User() { }

    /// <summary>
    /// Tạo user mới với email/password
    /// </summary>
    public static User CreateWithEmail(int id, string email, string password, string fullname)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email không được để trống", nameof(email));
        
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Mật khẩu không được để trống", nameof(password));
            
        if (string.IsNullOrWhiteSpace(fullname))
            throw new ArgumentException("Họ tên không được để trống", nameof(fullname));

        // Validate email format
        if (!IsValidEmail(email))
            throw new ArgumentException("Email không đúng định dạng", nameof(email));

        // Validate password strength
        if (password.Length < 6)
            throw new ArgumentException("Mật khẩu phải có ít nhất 6 ký tự", nameof(password));

        var user = new User
        {
            Id = id,
            Email = email.ToLower().Trim(),
            Fullname = fullname.Trim(),
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            LoginMethod = "Email",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add domain event
        user._domainEvents.Add(new UserCreatedEvent(
            user.Id,
            user.Email,
            user.Fullname
        ));

        return user;
    }

    /// <summary>
    /// Tạo user mới với Google OAuth
    /// </summary>
    public static User CreateWithGoogle(int id, string email, string fullname, string googleId)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email không được để trống", nameof(email));
            
        if (string.IsNullOrWhiteSpace(fullname))
            throw new ArgumentException("Họ tên không được để trống", nameof(fullname));

        if (string.IsNullOrWhiteSpace(googleId))
            throw new ArgumentException("Google ID không được để trống", nameof(googleId));

        // Validate email format
        if (!IsValidEmail(email))
            throw new ArgumentException("Email không đúng định dạng", nameof(email));

        var user = new User
        {
            Id = id,
            Email = email.ToLower().Trim(),
            Fullname = fullname.Trim(),
            Password = string.Empty, // No password for Google login
            LoginMethod = "Google",
            GoogleId = googleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add domain event
        user._domainEvents.Add(new UserCreatedEvent(
            user.Id,
            user.Email,
            user.Fullname
        ));

        return user;
    }

    /// <summary>
    /// Xác thực mật khẩu (chỉ cho email login)
    /// </summary>
    public bool VerifyPassword(string password)
    {
        if (LoginMethod != "Email" || string.IsNullOrEmpty(Password))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, Password);
    }

    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    public void ChangePassword(string currentPassword, string newPassword)
    {
        if (LoginMethod != "Email")
            throw new InvalidOperationException("Chỉ có thể đổi mật khẩu cho tài khoản đăng ký bằng email");

        if (!VerifyPassword(currentPassword))
            throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng");

        if (newPassword.Length < 6)
            throw new ArgumentException("Mật khẩu mới phải có ít nhất 6 ký tự", nameof(newPassword));

        Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cập nhật thông tin cơ bản
    /// </summary>
    public void UpdateBasicInfo(string fullname)
    {
        if (string.IsNullOrWhiteSpace(fullname))
            throw new ArgumentException("Họ tên không được để trống", nameof(fullname));

        Fullname = fullname.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Đặt role cho user
    /// </summary>
    public void SetRole(string role)
    {
        var validRoles = new[] { "User", "Admin", "Partner" };
        if (!validRoles.Contains(role))
            throw new ArgumentException($"Role không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validRoles)}", nameof(role));

        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Kích hoạt/vô hiệu hóa tài khoản
    /// </summary>
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Tạo profile cho user (Note: ID phải được generate bởi repository)
    /// </summary>
    public Profile CreateProfile(int profileId, string? gender = null, string? avatar = null, 
        string? phoneNumber = null, string? address = null)
    {
        if (Profile != null)
            throw new InvalidOperationException("User đã có profile");

        Profile = Profile.Create(profileId, Id, gender, avatar, phoneNumber, address);
        UpdatedAt = DateTime.UtcNow;

        return Profile;
    }

    /// <summary>
    /// Thêm notification cho user (Note: ID phải được generate bởi repository)
    /// </summary>
    public Notification AddNotification(int notificationId, string title, string content)
    {
        var notification = Notification.Create(notificationId, Id, title, content);
        Notifications.Add(notification);
        
        _domainEvents.Add(new NotificationCreatedEvent(
            notification.Id,
            Id,
            title,
            content
        ));

        return notification;
    }

    /// <summary>
    /// Clear domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Validate email format
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
