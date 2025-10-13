using MongoDB.Bson;
using MongoDB.Driver;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;

namespace Mumii.Social.Infrastructure.Repositories;

public class MoodRepository : IMoodRepository
{
    private readonly IMongoCollection<Mood> _moods;

    public MoodRepository(IMongoDatabase database)
    {
        _moods = database.GetCollection<Mood>("moods");
    }

    public async Task<Mood?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _moods.Find(m => m.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Mood>> GetAllAsync(int skip = 0, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _moods.Find(_ => true).Skip(skip).Limit(limit).ToListAsync(cancellationToken);
    }

    public async Task<Mood> AddAsync(Mood mood, CancellationToken cancellationToken = default)
    {
        if (mood.Id == 0)
        {
            var counters = _moods.Database.GetCollection<BsonDocument>("counters");
            var result = await counters.FindOneAndUpdateAsync(
                Builders<BsonDocument>.Filter.Eq("_id", "moods"),
                Builders<BsonDocument>.Update.Inc("seq", 1),
                new FindOneAndUpdateOptions<BsonDocument> { IsUpsert = true, ReturnDocument = ReturnDocument.After },
                cancellationToken);
            var nextId = result.GetValue("seq", 1).AsInt32;
            var created = Mood.Create(mood.Name, mood.Description);
            typeof(Mood).GetProperty("Id")!.SetValue(created, nextId);
            mood = created;
        }
        await _moods.InsertOneAsync(mood, cancellationToken: cancellationToken);
        return mood;
    }

    public async Task<Mood> UpdateAsync(Mood mood, CancellationToken cancellationToken = default)
    {
        await _moods.ReplaceOneAsync(m => m.Id == mood.Id, mood, cancellationToken: cancellationToken);
        return mood;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _moods.DeleteOneAsync(m => m.Id == id, cancellationToken);
    }
}


