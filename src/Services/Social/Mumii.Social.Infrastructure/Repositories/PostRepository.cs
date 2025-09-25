using Microsoft.EntityFrameworkCore;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using Mumii.Social.Infrastructure.Data;
using Mumii.Shared.Common.DTOs;

namespace Mumii.Social.Infrastructure.Repositories;

/// <summary>
/// Implementation của IPostRepository
/// </summary>
public class PostRepository : IPostRepository
{
    private readonly SocialDbContext _context;

    public PostRepository(SocialDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tìm bài đăng theo ID
    /// </summary>
    public async Task<Post?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Include(p => p.Comments.Where(c => !c.IsDeleted))
            .Include(p => p.Reactions)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Lấy danh sách bài đăng có phân trang
    /// </summary>
    public async Task<PagedResult<Post>> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Posts
            .Where(p => !p.IsDeleted)
            .Include(p => p.Reactions)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
        IQueryable<Post> dbQuery = _context.Posts
            .Where(p => !p.IsDeleted)
            .Include(p => p.Reactions);

        // Lọc theo mood
        if (!string.IsNullOrWhiteSpace(query.Mood))
        {
            dbQuery = dbQuery.Where(p => p.Mood == query.Mood);
        }

        // Lọc theo restaurant ID
        if (!string.IsNullOrWhiteSpace(query.RestaurantId))
        {
            dbQuery = dbQuery.Where(p => p.RestaurantId == query.RestaurantId);
        }

        // Lọc theo account ID
        if (!string.IsNullOrWhiteSpace(query.AccountId))
        {
            dbQuery = dbQuery.Where(p => p.AccountId == query.AccountId);
        }

        // Lọc theo ngày tạo
        if (query.FromDate.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.CreatedAt >= query.FromDate);
        }

        if (query.ToDate.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.CreatedAt <= query.ToDate);
        }

        // Sắp xếp
        dbQuery = dbQuery.OrderByDescending(p => p.CreatedAt);

        var totalCount = await dbQuery.CountAsync(cancellationToken);
        
        var items = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
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
        var query = _context.Posts
            .Where(p => p.AccountId == accountId && !p.IsDeleted)
            .Include(p => p.Reactions)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<Post>(items, totalCount, page, pageSize, totalPages);
    }

    /// <summary>
    /// Thêm bài đăng mới
    /// </summary>
    public async Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        await _context.Posts.AddAsync(post, cancellationToken);
        return post;
    }

    /// <summary>
    /// Cập nhật bài đăng
    /// </summary>
    public async Task<Post> UpdateAsync(Post post, CancellationToken cancellationToken = default)
    {
        _context.Posts.Update(post);
        return await Task.FromResult(post);
    }

    /// <summary>
    /// Xóa bài đăng (soft delete)
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var post = await GetByIdAsync(id, cancellationToken);
        if (post != null)
        {
            post.Delete();
            _context.Posts.Update(post);
        }
    }

    /// <summary>
    /// Kiểm tra bài đăng có tồn tại không
    /// </summary>
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .AnyAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
