using Mumii.Auth.Domain.Entities;
using Mumii.Shared.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Mumii.Auth.Domain.Interfaces;

/// <summary>
/// Repository interface cho User entity (MongoDB)
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Tìm user theo email
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm user theo ID
    /// </summary>
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm user theo Google ID
    /// </summary>
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm user theo refresh token
    /// </summary>
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm user mới
    /// </summary>
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật user
    /// </summary>
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra email đã tồn tại chưa
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả users (với phân trang)
    /// </summary>
    Task<List<User>> GetAllAsync(int skip = 0, int limit = 100, CancellationToken cancellationToken = default);

    Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    // >>> THÊM PHƯƠNG THỨC MỚI <<<
    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
}
