using MongoDB.Driver;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;

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

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.GoogleId, googleId);
        return await _users.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.RefreshToken, refreshToken);
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
}

