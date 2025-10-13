using MongoDB.Driver;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;

namespace Mumii.Auth.Infrastructure.Repositories;

/// <summary>
/// Implementation của INotificationRepository với MongoDB
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _notifications;

    public NotificationRepository(IMongoDatabase database)
    {
        _notifications = database.GetCollection<Notification>("notifications");
    }

    public async Task<Notification?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
        return await _notifications.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Notification>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
        return await _notifications.Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Notification>> GetUnreadByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter.Eq(n => n.UserId, userId),
            Builders<Notification>.Filter.Eq(n => n.IsRead, false)
        );
        return await _notifications.Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _notifications.InsertOneAsync(notification, cancellationToken: cancellationToken);
        return notification;
    }

    public async Task<Notification> UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, notification.Id);
        await _notifications.ReplaceOneAsync(filter, notification, cancellationToken: cancellationToken);
        return notification;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
        await _notifications.DeleteOneAsync(filter, cancellationToken);
    }
}

