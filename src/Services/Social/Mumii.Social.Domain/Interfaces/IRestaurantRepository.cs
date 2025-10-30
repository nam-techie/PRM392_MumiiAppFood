using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mumii.Shared.Common.DTOs;

namespace Mumii.Social.Domain.Interfaces
{
    /// <summary>
    /// Repository cho truy vấn Restaurant từ service khác (cross-service)
    /// </summary>
    public interface IRestaurantRepository
    {
        /// <summary>
        /// Lấy danh sách nhà hàng theo List Ids
        /// </summary>
        Task<List<RestaurantDto>> GetByIdsAsync(IEnumerable<int> restaurantIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy nhà hàng theo Id
        /// </summary>
        Task<RestaurantDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
