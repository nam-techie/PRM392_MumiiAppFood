using MongoDB.Driver;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using Mumii.Shared.Common.DTOs;
using MongoDB.Bson;

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

    /// <summary>
    /// Tìm bài đăng theo ID
    /// </summary>
    public async Task<Post?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (int.TryParse(id, out var pid))
            return await _posts.Find(p => p.Id == pid).FirstOrDefaultAsync(cancellationToken);
        return null;
    }

    /// <summary>
    /// Lấy danh sách bài đăng có phân trang
    /// </summary>
    public async Task<PagedResult<Post>> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var find = _posts.Find(_ => true);
        var totalCount = (int)await find.CountDocumentsAsync(cancellationToken);
        var items = await find.SortByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<Post>(items, totalCount, page, pageSize, totalPages);
    }

    /// <summary>
    /// Tìm kiếm bài đăng
    /// </summary>
    public async Task<PagedResult<Post>> SearchAsync(
        SearchPostsQuery query, 
        CancellationToken cancellationToken = default)
    {
        var filters = new List<FilterDefinition<Post>>();
        var builder = Builders<Post>.Filter;
        if (query.RestaurantId.HasValue) filters.Add(builder.Eq(p => p.RestaurantId, query.RestaurantId.Value));
        if (query.PartnerId.HasValue) filters.Add(builder.Eq(p => p.PartnerId, query.PartnerId.Value));
        var filter = filters.Count == 0 ? builder.Empty : builder.And(filters);
        var totalCount = (int)await _posts.CountDocumentsAsync(filter, cancellationToken);
        var items = await _posts.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        return new PagedResult<Post>(items, totalCount, query.Page, query.PageSize, totalPages);
    }

    /// <summary>
    /// Lấy bài đăng của một user
    /// </summary>
    public async Task<PagedResult<Post>> GetByAccountIdAsync(
        string accountId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(accountId, out var partnerId))
            return new PagedResult<Post>(new List<Post>(), 0, page, pageSize, 0);

        var filter = Builders<Post>.Filter.Eq(p => p.PartnerId, partnerId);
        var totalCount = (int)await _posts.CountDocumentsAsync(filter, cancellationToken);
        var items = await _posts.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<Post>(items, totalCount, page, pageSize, totalPages);
    }

    /// <summary>
    /// Thêm bài đăng mới
    /// </summary>
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

    /// <summary>
    /// Cập nhật bài đăng
    /// </summary>
    public async Task<Post> UpdateAsync(Post post, CancellationToken cancellationToken = default)
    {
        await _posts.ReplaceOneAsync(p => p.Id == post.Id, post, cancellationToken: cancellationToken);
        return post;
    }

    /// <summary>
    /// Xóa bài đăng (soft delete)
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(id, out var pid)) return;
        await _posts.DeleteOneAsync(p => p.Id == pid, cancellationToken);
    }

    /// <summary>
    /// Kiểm tra bài đăng có tồn tại không
    /// </summary>
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(id, out var pid)) return false;
        var count = await _posts.CountDocumentsAsync(p => p.Id == pid, cancellationToken: cancellationToken);
        return count > 0;
    }

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

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
