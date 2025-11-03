using MongoDB.Bson;
using MongoDB.Driver;

namespace Mumii.Auth.Infrastructure.Services;

/// <summary>
/// Service để generate auto-increment IDs cho MongoDB collections
/// </summary>
public interface IMongoIdGenerator
{
    Task<int> GetNextIdAsync(string collectionName, CancellationToken cancellationToken = default);
}

public class MongoIdGenerator : IMongoIdGenerator
{
    private readonly IMongoCollection<BsonDocument> _counters;

    public MongoIdGenerator(IMongoDatabase database)
    {
        _counters = database.GetCollection<BsonDocument>("counters");
    }

    public async Task<int> GetNextIdAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", collectionName);
        var update = Builders<BsonDocument>.Update.Inc("seq", 1);
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var result = await _counters.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        return result["seq"].AsInt32;
    }
}

