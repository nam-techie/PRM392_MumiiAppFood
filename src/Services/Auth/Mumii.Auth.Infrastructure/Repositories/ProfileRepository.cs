using MongoDB.Driver;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mumii.Auth.Infrastructure.Repositories;

/// <summary>
/// Implementation của IProfileRepository với MongoDB
/// </summary>
public class ProfileRepository : IProfileRepository
{
    private readonly IMongoCollection<Profile> _profiles;

    public ProfileRepository(IMongoDatabase database)
    {
        _profiles = database.GetCollection<Profile>("profiles");
    }

    public async Task<Profile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Profile>.Filter.Eq(p => p.Id, id);
        return await _profiles.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Profile?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Profile>.Filter.Eq(p => p.UserId, userId);
        return await _profiles.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Profile>> GetByUserIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Profile>.Filter.In(p => p.UserId, userIds);
        return await _profiles.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<Profile> AddAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        await _profiles.InsertOneAsync(profile, cancellationToken: cancellationToken);
        return profile;
    }

    public async Task<Profile> UpdateAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Profile>.Filter.Eq(p => p.Id, profile.Id);
        await _profiles.ReplaceOneAsync(filter, profile, cancellationToken: cancellationToken);
        return profile;
    }
}
