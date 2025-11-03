using MongoDB.Driver;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Shared.Common.DTOs;
using Mumii.Shared.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Mumii.Auth.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _notifications;

    public NotificationRepository(IMongoDatabase database)
    {
        _notifications = database.GetCollection<Notification>("notifications");
    }

    public async Task<Notification?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _notifications.Find(n => n.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Sắp xếp để thông báo mới nhất lên đầu
        return await _notifications.Find(n => n.UserId == userId)
                                   .SortByDescending(n => n.CreatedAt)
                                   .ToListAsync(cancellationToken);
    }

    public async Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _notifications.InsertOneAsync(notification, cancellationToken: cancellationToken);
        return notification;
    }

    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _notifications.ReplaceOneAsync(n => n.Id == notification.Id, notification, cancellationToken: cancellationToken);
    }

    public async Task UpdateManyAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
    {
        var updates = new List<WriteModel<Notification>>();
        foreach (var notification in notifications)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notification.Id);
            var update = new ReplaceOneModel<Notification>(filter, notification);
            updates.Add(update);
        }
        if (updates.Any())
        {
            await _notifications.BulkWriteAsync(updates, cancellationToken: cancellationToken);
        }
    }
    
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _notifications.DeleteOneAsync(n => n.Id == id, cancellationToken);
    }

    // >>> TRIỂN KHAI CÁC PHƯƠNG THỨC MỚI <<<
    public async Task<PagedResult<Notification>> GetPagedAsync(int page, int pageSize, int? userId = null, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Notification>.Filter;
        var filter = userId.HasValue ? filterBuilder.Eq(n => n.UserId, userId.Value) : filterBuilder.Empty;

        var find = _notifications.Find(filter);

        var totalCount = (int)await find.CountDocumentsAsync(cancellationToken);

        var items = await find.SortByDescending(n => n.CreatedAt)
                              .Skip((page - 1) * pageSize)
                              .Limit(pageSize)
                              .ToListAsync(cancellationToken);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<Notification>(items, totalCount, page, pageSize, totalPages);
    }

    public async Task AddManyAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
    {
        if (notifications.Any())
        {
            await _notifications.InsertManyAsync(notifications, cancellationToken: cancellationToken);
        }
    }
}
