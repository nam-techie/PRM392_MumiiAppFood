using MongoDB.Driver;
using Mumii.Social.Domain.Interfaces;

namespace Mumii.Social.Infrastructure.Repositories;

public class PostMoodRepository : IPostMoodRepository
{
    private readonly IMongoCollection<dynamic> _postMoods;

    public PostMoodRepository(IMongoDatabase database)
    {
        _postMoods = database.GetCollection<dynamic>("post_moods");
    }

    public async Task<bool> ExistsAsync(int postId, int moodId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<dynamic>.Filter.Eq("post_id", postId) & Builders<dynamic>.Filter.Eq("mood_id", moodId);
        var count = await _postMoods.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task AddAsync(int postId, int moodId, CancellationToken cancellationToken = default)
    {
        await _postMoods.InsertOneAsync(new { post_id = postId, mood_id = moodId }, cancellationToken: cancellationToken);
    }

    public async Task RemoveAsync(int postId, int moodId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<dynamic>.Filter.Eq("post_id", postId) & Builders<dynamic>.Filter.Eq("mood_id", moodId);
        await _postMoods.DeleteOneAsync(filter, cancellationToken);
    }
}


