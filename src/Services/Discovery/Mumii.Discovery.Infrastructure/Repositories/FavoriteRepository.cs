using MongoDB.Driver;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mumii.Discovery.Infrastructure.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly IMongoCollection<Favorite> _favorites;

    public FavoriteRepository(IMongoDatabase database)
    {
        _favorites = database.GetCollection<Favorite>("favorites");
    }

    public async Task<Favorite?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _favorites.Find(f => f.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int userId, int restaurantId, CancellationToken cancellationToken = default)
    {
        var count = await _favorites.CountDocumentsAsync(f => f.UserId == userId && f.RestaurantId == restaurantId, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<List<Favorite>> GetByUserAsync(int userId, int skip = 0, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _favorites.Find(f => f.UserId == userId)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Favorite?> GetByUserAndRestaurantAsync(int userId, int restaurantId, CancellationToken cancellationToken = default)
    {
        return await _favorites.Find(f => f.UserId == userId && f.RestaurantId == restaurantId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Favorite> AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
    {
        await _favorites.InsertOneAsync(favorite, cancellationToken: cancellationToken);
        return favorite;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _favorites.DeleteOneAsync(f => f.Id == id, cancellationToken);
    }
}
