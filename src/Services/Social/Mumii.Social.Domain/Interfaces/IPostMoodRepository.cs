namespace Mumii.Social.Domain.Interfaces;

public interface IPostMoodRepository
{
    Task<bool> ExistsAsync(int postId, int moodId, CancellationToken cancellationToken = default);
    Task AddAsync(int postId, int moodId, CancellationToken cancellationToken = default);
    Task RemoveAsync(int postId, int moodId, CancellationToken cancellationToken = default);
}


