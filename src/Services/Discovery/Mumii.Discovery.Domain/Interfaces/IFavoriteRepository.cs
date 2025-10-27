using Mumii.Discovery.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mumii.Discovery.Domain.Interfaces;

public interface IFavoriteRepository
{
    Task<Favorite?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int userId, int restaurantId, CancellationToken cancellationToken = default);
    Task<List<Favorite>> GetByUserAsync(int userId, int skip = 0, int limit = 50, CancellationToken cancellationToken = default);
    Task<Favorite> AddAsync(Favorite favorite, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Favorite?> GetByUserAndRestaurantAsync(int userId, int restaurantId, CancellationToken cancellationToken = default);
}
