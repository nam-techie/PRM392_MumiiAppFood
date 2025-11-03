using Mumii.Social.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Mumii.Social.Domain.Interfaces;
public interface IMoodRepository
{
    Task<Mood?> GetByIdAsync(int id);
    Task<IEnumerable<Mood>> GetAllAsync();
    Task<Mood> AddAsync(Mood mood);
    Task UpdateAsync(Mood mood);
    Task DeleteAsync(int id);
    Task<IEnumerable<Mood>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
}