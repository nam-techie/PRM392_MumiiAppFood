using MongoDB.Driver;
using Mumii.Discovery.Domain.Entities;
using Mumii.Discovery.Domain.Interfaces;
using Mumii.Shared.Common.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // Thêm using này nếu cần

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

    public async Task<PagedResult<Review>> GetByRestaurantIdAsync(int restaurantId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var find = _reviews.Find(r => r.RestaurantId == restaurantId);

        var totalCount = (int)await find.CountDocumentsAsync(cancellationToken);

        var items = await find.SortByDescending(r => r.CreatedAt)
                              .Skip((page - 1) * pageSize)
                              .Limit(pageSize)
                              .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<Review>(items, totalCount, page, pageSize, totalPages);
    }

    public async Task UpdateAsync(Review review, CancellationToken cancellationToken = default)
    {
        await _reviews.ReplaceOneAsync(r => r.Id == review.Id, review, cancellationToken: cancellationToken);
    }

    // PHẦN TRIỂN KHAI MỚI
    public async Task<Review> AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        await _reviews.InsertOneAsync(review, cancellationToken: cancellationToken);
        return review;
    }

    public async Task DeleteAsync(int reviewId, CancellationToken cancellationToken = default)
    {
        await _reviews.DeleteOneAsync(r => r.Id == reviewId, cancellationToken);
    }
}