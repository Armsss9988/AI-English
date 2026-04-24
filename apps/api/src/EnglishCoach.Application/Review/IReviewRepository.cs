using EnglishCoach.Domain.Review;

namespace EnglishCoach.Application.Review;

public interface IReviewRepository
{
    Task<ReviewItem?> GetByCompositeKeyAsync(string userId, string itemId, ReviewTrack reviewTrack, CancellationToken cancellationToken);
    Task<ReviewItem?> GetByIdAsync(string reviewItemId, string userId, CancellationToken cancellationToken);
    Task CreateAsync(ReviewItem item, CancellationToken cancellationToken);
    Task UpdateAsync(ReviewItem item, CancellationToken cancellationToken);
    Task CompleteAsync(ReviewItem item, ReviewAttempt attempt, CancellationToken cancellationToken);
    Task<IReadOnlyList<DueReviewItemReadModel>> GetDueItemsAsync(string userId, DateTimeOffset nowUtc, CancellationToken cancellationToken);
}
