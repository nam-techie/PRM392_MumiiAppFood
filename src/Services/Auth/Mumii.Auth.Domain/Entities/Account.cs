using Mumii.Shared.Common.Enums;
using Mumii.Shared.Common.Events;

namespace Mumii.Auth.Domain.Entities;

/// <summary>
/// Entity tài khoản người dùng
/// </summary>
public class Account
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public UserRole Role { get; private set; } = UserRole.User;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Domain events
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Constructor cho Entity Framework
    /// </summary>
    private Account() { }

    /// <summary>
    /// Tạo tài khoản mới
    /// </summary>
    public static Account Create(string email, string password, string displayName)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email không được để trống", nameof(email));
        
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Mật khẩu không được để trống", nameof(password));
            
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Tên hiển thị không được để trống", nameof(displayName));

        // Validate email format
        if (!IsValidEmail(email))
            throw new ArgumentException("Email không đúng định dạng", nameof(email));

        // Validate password strength
        if (password.Length < 6)
            throw new ArgumentException("Mật khẩu phải có ít nhất 6 ký tự", nameof(password));

        var account = new Account
        {
            Email = email.ToLower().Trim(),
            DisplayName = displayName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add domain event
        account._domainEvents.Add(new AccountCreatedEvent(
            account.Id,
            account.Email,
            account.DisplayName
        ));

        return account;
    }

    /// <summary>
    /// Xác thực mật khẩu
    /// </summary>
    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }

    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    public void ChangePassword(string currentPassword, string newPassword)
    {
        if (!VerifyPassword(currentPassword))
            throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng");

        if (newPassword.Length < 6)
            throw new ArgumentException("Mật khẩu mới phải có ít nhất 6 ký tự", nameof(newPassword));

        PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cập nhật profile
    /// </summary>
    public void UpdateProfile(string displayName, string? avatarUrl = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Tên hiển thị không được để trống", nameof(displayName));

        DisplayName = displayName.Trim();
        AvatarUrl = avatarUrl?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Đặt role admin
    /// </summary>
    public void SetAdminRole()
    {
        Role = UserRole.Admin;
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
