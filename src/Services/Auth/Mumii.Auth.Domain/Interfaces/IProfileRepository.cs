using Mumii.Auth.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Mumii.Auth.Domain.Interfaces;

/// <summary>
/// Repository interface cho Profile entity (MongoDB)
/// </summary>
public interface IProfileRepository
{
    /// <summary>
    /// Tìm profile theo ID
    /// </summary>
    Task<Profile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm profile theo User ID
    /// </summary>
    Task<Profile?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm profile mới
    /// </summary>
    Task<Profile> AddAsync(Profile profile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật profile
    /// </summary>
    Task<Profile> UpdateAsync(Profile profile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm nhiều profiles theo danh sách User IDs
    /// </summary>
    Task<IEnumerable<Profile>> GetByUserIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);
}
