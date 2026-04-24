using Microsoft.EntityFrameworkCore;
using EnglishCoach.Application.Review;
using EnglishCoach.Domain.Review;
using EnglishCoach.Infrastructure.Persistence;

namespace EnglishCoach.Infrastructure.Review;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly EnglishCoachDbContext _dbContext;

    public ReviewRepository(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ReviewItem?> GetByCompositeKeyAsync(string userId, string itemId, ReviewTrack reviewTrack, CancellationToken cancellationToken)
    {
        return _dbContext.ReviewItems.SingleOrDefaultAsync(
            item => item.UserId == userId && item.ItemId == itemId && item.ReviewTrack == reviewTrack,
            cancellationToken);
    }

    public Task<ReviewItem?> GetByIdAsync(string reviewItemId, string userId, CancellationToken cancellationToken)
    {
        return _dbContext.ReviewItems.SingleOrDefaultAsync(
            item => item.Id == reviewItemId && item.UserId == userId,
            cancellationToken);
    }

    public async Task CreateAsync(ReviewItem item, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.ReviewItems.AddAsync(item, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            throw new DuplicateReviewItemException();
        }
    }

    public async Task UpdateAsync(ReviewItem item, CancellationToken cancellationToken)
    {
        _dbContext.ReviewItems.Update(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAsync(ReviewItem item, ReviewAttempt attempt, CancellationToken cancellationToken)
    {
        await _dbContext.ReviewAttempts.AddAsync(attempt, cancellationToken);
        _dbContext.ReviewItems.Update(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DueReviewItemReadModel>> GetDueItemsAsync(string userId, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        return await _dbContext.ReviewItems
            .Where(item => item.UserId == userId && item.DueAtUtc <= nowUtc)
            .OrderBy(item => item.DueAtUtc)
            .ThenBy(item => item.MasteryState)
            .Select(item => new DueReviewItemReadModel(
                item.Id,
                item.ItemId,
                item.ReviewTrack.ToString().ToLowerInvariant(),
                item.DisplayText,
                item.DisplaySubtitle,
                item.MasteryState.ToString().ToLowerInvariant().Replace("clientready", "client_ready"),
                item.RepetitionCount,
                item.DueAtUtc))
            .ToListAsync(cancellationToken);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate key value violates unique constraint", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_review_items_user_id_item_id_review_track", StringComparison.OrdinalIgnoreCase);
    }
}
