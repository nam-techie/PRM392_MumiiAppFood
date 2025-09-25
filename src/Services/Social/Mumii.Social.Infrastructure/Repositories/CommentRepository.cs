using Microsoft.EntityFrameworkCore;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using Mumii.Social.Infrastructure.Data;

namespace Mumii.Social.Infrastructure.Repositories;

/// <summary>
/// Implementation của ICommentRepository
/// </summary>
public class CommentRepository : ICommentRepository
{
    private readonly SocialDbContext _context;

    public CommentRepository(SocialDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tìm comment theo ID
    /// </summary>
    public async Task<Comment?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Lấy comments của một post
    /// </summary>
    public async Task<List<Comment>> GetByPostIdAsync(string postId, CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId && !c.IsDeleted)
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Thêm comment mới
    /// </summary>
    public async Task<Comment> AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await _context.Comments.AddAsync(comment, cancellationToken);
        return comment;
    }

    /// <summary>
    /// Cập nhật comment
    /// </summary>
    public async Task<Comment> UpdateAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _context.Comments.Update(comment);
        return await Task.FromResult(comment);
    }

    /// <summary>
    /// Xóa comment (soft delete)
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var comment = await GetByIdAsync(id, cancellationToken);
        if (comment != null)
        {
            comment.Delete();
            _context.Comments.Update(comment);
        }
    }

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
