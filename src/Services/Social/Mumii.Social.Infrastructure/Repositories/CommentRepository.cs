using MongoDB.Bson;
using MongoDB.Driver;
using Mumii.Social.Domain.Entities;
using Mumii.Social.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mumii.Social.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly IMongoCollection<Comment> _comments;

    public CommentRepository(IMongoDatabase database)
    {
        _comments = database.GetCollection<Comment>("comments");
    }

    public async Task<Comment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _comments.Find(c => c.Id == id && !c.IsDeleted).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Comment>> GetByPostIdAsync(int postId, CancellationToken cancellationToken = default)
    {
        return await _comments.Find(c => c.PostId == postId && !c.IsDeleted)
                              .SortBy(c => c.CreatedAt)
                              .ToListAsync(cancellationToken);
    }

    public async Task<Comment> AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        var nextId = await GetNextIdAsync("comments", cancellationToken);
        
        var commentToInsert = Comment.Create(nextId, comment.PostId, comment.UserId, comment.Content);

        await _comments.InsertOneAsync(commentToInsert, cancellationToken: cancellationToken);
        return commentToInsert;
    }

    public async Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await _comments.ReplaceOneAsync(c => c.Id == comment.Id, comment, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.Id, id);
        var update = Builders<Comment>.Update.Set(c => c.IsDeleted, true);
        await _comments.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
    
    // Hàm helper để tạo ID tuần tự
    private async Task<int> GetNextIdAsync(string sequenceName, CancellationToken cancellationToken)
    {
        var counters = _comments.Database.GetCollection<BsonDocument>("counters");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", sequenceName);
        var update = Builders<BsonDocument>.Update.Inc("seq", 1);
        var options = new FindOneAndUpdateOptions<BsonDocument> { IsUpsert = true, ReturnDocument = ReturnDocument.After };
        var result = await counters.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        return result.GetValue("seq", 0).AsInt32 + 1;
    }
}
