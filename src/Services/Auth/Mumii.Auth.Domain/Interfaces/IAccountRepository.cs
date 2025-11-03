using Mumii.Auth.Domain.Entities;

namespace Mumii.Auth.Domain.Interfaces;

/// <summary>
/// Repository interface cho Account entity
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Tìm tài khoản theo email
    /// </summary>
    Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm tài khoản theo ID
    /// </summary>
    Task<Account?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm tài khoản mới
    /// </summary>
    Task<Account> AddAsync(Account account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật tài khoản
    /// </summary>
    Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra email đã tồn tại chưa
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
