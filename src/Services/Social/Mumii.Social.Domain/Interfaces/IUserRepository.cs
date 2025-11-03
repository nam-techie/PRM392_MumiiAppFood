using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mumii.Shared.Common.DTOs;

namespace Mumii.Social.Domain.Interfaces
{
    /// <summary>
    /// Repository cho truy vấn User/Partner từ service khác (cross-service)
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Lấy danh sách user/partner theo List Ids
        /// </summary>
        Task<List<UserDto>> GetByIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy user/partner theo Id
        /// </summary>
        Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
