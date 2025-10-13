using Mumii.Discovery.Domain.Entities;

namespace Mumii.Discovery.Domain.Interfaces;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Review>> GetByRestaurantAsync(int restaurantId, int skip = 0, int limit = 50, CancellationToken cancellationToken = default);
    Task<Review> AddAsync(Review review, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}


