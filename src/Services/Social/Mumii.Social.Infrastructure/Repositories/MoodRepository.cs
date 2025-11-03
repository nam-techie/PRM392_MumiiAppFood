using MongoDB.Driver;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using MongoDB.Bson;

namespace Mumii.Social.Infrastructure.Repositories;

public class MoodRepository : IMoodRepository
{
    private readonly IMongoCollection<Mood> _moods;

    public MoodRepository(IMongoDatabase database)
    {
        _moods = database.GetCollection<Mood>("moods");
    }

    public async Task<Mood?> GetByIdAsync(int id)
    {
        return await _moods.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Mood>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Mood>.Filter.In(m => m.Id, ids);
        return await _moods.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Mood>> GetAllAsync()
    {
        return await _moods.Find(_ => true).ToListAsync();
    }

    public async Task<Mood> AddAsync(Mood mood)
    {
        var nextId = await GetNextIdAsync("moods");
        var moodToInsert = Mood.Create(nextId, mood.Name, mood.Description);
        await _moods.InsertOneAsync(moodToInsert);
        return moodToInsert;
    }

    public async Task UpdateAsync(Mood mood)
    {
        await _moods.ReplaceOneAsync(m => m.Id == mood.Id, mood);
    }

    public async Task DeleteAsync(int id)
    {
        await _moods.DeleteOneAsync(m => m.Id == id);
    }

    private async Task<int> GetNextIdAsync(string sequenceName, CancellationToken cancellationToken = default)
    {
        var counters = _moods.Database.GetCollection<BsonDocument>("counters");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", sequenceName);
        var update = Builders<BsonDocument>.Update.Inc("seq", 1);
        var options = new FindOneAndUpdateOptions<BsonDocument> { IsUpsert = true, ReturnDocument = ReturnDocument.After };
        var result = await counters.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        return result.GetValue("seq", 0).AsInt32 + 1; // Logic an toàn hơn
    }
}
