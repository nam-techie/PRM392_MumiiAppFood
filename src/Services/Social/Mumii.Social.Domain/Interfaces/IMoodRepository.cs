using Mumii.Social.Domain.Entities;

namespace Mumii.Social.Domain.Interfaces;

public interface IMoodRepository
{
    Task<Mood?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Mood>> GetAllAsync(int skip = 0, int limit = 100, CancellationToken cancellationToken = default);
    Task<Mood> AddAsync(Mood mood, CancellationToken cancellationToken = default);
    Task<Mood> UpdateAsync(Mood mood, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}


