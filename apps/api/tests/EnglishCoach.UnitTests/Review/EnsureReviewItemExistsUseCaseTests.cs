using EnglishCoach.Application.Review;
using EnglishCoach.Contracts.Review;
using EnglishCoach.Domain.Review;
using EnglishCoach.SharedKernel.Time;

namespace EnglishCoach.UnitTests.Review;

public sealed class EnsureReviewItemExistsUseCaseTests
{
    [Fact]
    public async Task Returns_Existing_Item_When_Create_Hits_Duplicate()
    {
        var existing = ReviewItem.Create(
            "review-1",
            "user-1",
            "phrase-1",
            ReviewTrack.Phrase,
            "Could you please clarify?",
            "subtitle",
            new DateTimeOffset(2026, 4, 24, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 24, 9, 0, 0, TimeSpan.Zero));
        var repository = new DuplicateOnCreateRepository(existing);
        var useCase = new EnsureReviewItemExistsUseCase(repository, new StubClock(existing.CreatedAtUtc));

        var response = await useCase.ExecuteAsync(
            new EnsureReviewItemRequest("user-1", "phrase-1", "phrase", "Could you please clarify?", "subtitle"),
            CancellationToken.None);

        Assert.Equal("review-1", response.ReviewItemId);
    }

    private sealed class DuplicateOnCreateRepository : IReviewRepository
    {
        private readonly ReviewItem _existing;
        private int _lookupCount;

        public DuplicateOnCreateRepository(ReviewItem existing)
        {
            _existing = existing;
        }

        public Task<ReviewItem?> GetByCompositeKeyAsync(string userId, string itemId, ReviewTrack reviewTrack, CancellationToken cancellationToken)
        {
            _lookupCount++;
            return Task.FromResult(_lookupCount == 1 ? null : _existing);
        }

        public Task<ReviewItem?> GetByIdAsync(string reviewItemId, string userId, CancellationToken cancellationToken)
            => Task.FromResult<ReviewItem?>(null);

        public Task CreateAsync(ReviewItem item, CancellationToken cancellationToken)
            => Task.FromException(new DuplicateReviewItemException());

        public Task UpdateAsync(ReviewItem item, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task CompleteAsync(ReviewItem item, ReviewAttempt attempt, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<IReadOnlyList<DueReviewItemReadModel>> GetDueItemsAsync(string userId, DateTimeOffset nowUtc, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DueReviewItemReadModel>>(Array.Empty<DueReviewItemReadModel>());
    }

    private sealed class StubClock : IClock
    {
        public StubClock(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; }
    }
}
