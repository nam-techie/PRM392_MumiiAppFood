using MongoDB.Driver;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;

namespace Mumii.Discovery.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly IMongoCollection<Review> _reviews;

    public ReviewRepository(IMongoDatabase database)
    {
        _reviews = database.GetCollection<Review>("reviews");
    }

    public async Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Review>> GetByRestaurantAsync(int restaurantId, int skip = 0, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _reviews.Find(r => r.RestaurantId == restaurantId)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Review> AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        await _reviews.InsertOneAsync(review, cancellationToken: cancellationToken);
        return review;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _reviews.DeleteOneAsync(r => r.Id == id, cancellationToken);
    }
}


