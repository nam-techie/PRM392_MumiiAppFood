using Mumii.Auth.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mumii.Auth.Domain.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateManyAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
