using MongoDB.Driver;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using Mumii.Shared.Common.Models;
using MongoDB.Bson;
using Mumii.Shared.Common.DTOs;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Mumii.Social.Infrastructure.Repositories;

/// <summary>
/// Implementation của IPostRepository
/// </summary>
public class PostRepository : IPostRepository
{
    private readonly IMongoCollection<Post> _posts;

    public PostRepository(IMongoDatabase database)
    {
        _posts = database.GetCollection<Post>("posts");
    }

    public async Task<Post?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _posts.Find(p => p.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<Post>> GetPagedAsync(
        int page, 
        int pageSize,
        int? partnerId = null,
        string? status = null, 
        int? restaurantId = null, 
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Post>.Filter;
        var filters = new List<FilterDefinition<Post>>();
        
        if (partnerId.HasValue) filters.Add(filterBuilder.Eq(p => p.PartnerId, partnerId.Value));
        if (!string.IsNullOrWhiteSpace(status)) filters.Add(filterBuilder.Eq(p => p.Status, status));
        if (restaurantId.HasValue) filters.Add(filterBuilder.Eq(p => p.RestaurantId, restaurantId.Value));

        var filter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;

        var find = _posts.Find(filter);
        var totalCount = (int)await find.CountDocumentsAsync(cancellationToken);
        var items = await find.SortByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<Post>(items, totalCount, page, pageSize, totalPages);
    }

    public async Task<PagedResult<Post>> SearchAsync(SearchPostsQuery query, CancellationToken cancellationToken = default)
    {
        var filters = new List<FilterDefinition<Post>>();
        var builder = Builders<Post>.Filter;
        if (query.RestaurantId.HasValue) filters.Add(builder.Eq(p => p.RestaurantId, query.RestaurantId.Value));
        if (query.PartnerId.HasValue) filters.Add(builder.Eq(p => p.PartnerId, query.PartnerId.Value));
        var filter = filters.Count == 0 ? builder.Empty : builder.And(filters);
        var totalCount = (int)await _posts.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _posts.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        return new PagedResult<Post>(items, totalCount, query.Page, query.PageSize, totalPages);
    }

    public async Task<PagedResult<Post>> GetByAccountIdAsync(int accountId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Post>.Filter.Eq(p => p.PartnerId, accountId);
        var totalCount = (int)await _posts.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _posts.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<Post>(items, totalCount, page, pageSize, totalPages);
    }

    public async Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        if (post.Id == 0)
        {
            post = Post.Create(
                id: await GetNextIdAsync("posts", cancellationToken),
                partnerId: post.PartnerId,
                title: post.Title,
                content: post.Content,
                imageUrl: post.ImageUrl,
                restaurantId: post.RestaurantId
            );
        }
        await _posts.InsertOneAsync(post, cancellationToken: cancellationToken);
        return post;
    }

    public async Task UpdateAsync(Post post, CancellationToken cancellationToken = default)
    {
        await _posts.ReplaceOneAsync(p => p.Id == post.Id, post, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _posts.DeleteOneAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        var count = await _posts.CountDocumentsAsync(p => p.Id == id, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<bool> IsMoodInUseAsync(int moodId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Post>.Filter.ElemMatch(
            p => p.PostMoods, 
            pm => pm.MoodId == moodId
        );
        var post = await _posts.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return post != null;
    }

    private async Task<int> GetNextIdAsync(string sequenceName, CancellationToken cancellationToken)
    {
        var counters = _posts.Database.GetCollection<BsonDocument>("counters");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", sequenceName);
        var update = Builders<BsonDocument>.Update.Inc("seq", 1);
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        var result = await counters.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        return result.GetValue("seq", 1).AsInt32;
    }
}
