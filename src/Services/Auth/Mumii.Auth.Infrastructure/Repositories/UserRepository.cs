using MongoDB.Driver;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Shared.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Mumii.Auth.Infrastructure.Repositories;

/// <summary>
/// Implementation của IUserRepository với MongoDB
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("users");
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email.ToLower());
        return await _users.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        return await _users.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.In(u => u.Id, ids);
        return await _users.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.GoogleId, googleId);
        return await _users.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _users.InsertOneAsync(user, cancellationToken: cancellationToken);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
        await _users.ReplaceOneAsync(filter, user, cancellationToken: cancellationToken);
        return user;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email.ToLower());
        var count = await _users.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<List<User>> GetAllAsync(int skip = 0, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _users.Find(_ => true)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var find = _users.Find(_ => true); // Lấy tất cả user

        var totalCount = (int)await find.CountDocumentsAsync(cancellationToken);

        var items = await find.SortByDescending(u => u.CreatedAt)
                              .Skip((page - 1) * pageSize)
                              .Limit(pageSize)
                              .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<User>(items, totalCount, page, pageSize, totalPages);
    }
}
