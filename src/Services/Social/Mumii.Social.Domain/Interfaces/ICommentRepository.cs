using Mumii.Social.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mumii.Social.Domain.Interfaces;

/// <summary>
/// Repository interface cho Comment entity
/// </summary>
public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(int id, CancellationToken cancellationToken = default); // Sửa thành int
    Task<List<Comment>> GetByPostIdAsync(int postId, CancellationToken cancellationToken = default); // Sửa thành int
    Task<Comment> AddAsync(Comment comment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default); // Sửa thành int
}