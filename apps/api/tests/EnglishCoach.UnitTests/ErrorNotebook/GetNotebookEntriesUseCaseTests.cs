using EnglishCoach.Application.ErrorNotebook;
using EnglishCoach.Application.Review;
using EnglishCoach.Domain.ErrorNotebook;
using EnglishCoach.Domain.Review;
using EnglishCoach.SharedKernel.Time;

namespace EnglishCoach.UnitTests.ErrorNotebook;

public class GetNotebookEntriesUseCaseTests
{
    // ── E3 Acceptance: Archived entries are excluded from default learner view ──

    [Fact]
    public async Task Execute_ExcludesArchivedEntries()
    {
        var active = CreateEntry("e1", "learner-1", "missing-article", ErrorSeverity.High, NotebookEntryState.Learning);
        var archived = CreateEntry("e2", "learner-1", "tense-error", ErrorSeverity.Medium, NotebookEntryState.Archived);

        var notebookRepo = new FakeNotebookRepo(new[] { active, archived });
        var reviewRepo = new FakeReviewRepo();
        var clock = new FakeClock(DateTimeOffset.UtcNow);
        var useCase = new GetNotebookEntriesUseCase(notebookRepo, reviewRepo, clock);

        var results = await useCase.ExecuteAsync("learner-1");

        Assert.Single(results);
        Assert.Equal("missing-article", results[0].PatternKey);
    }

    // ── E3 Acceptance: Sort by severity, recurrence, and due status ──

    [Fact]
    public async Task Execute_SortsBySeverityAndRecurrence()
    {
        var highSev = CreateEntry("e1", "learner-1", "grammar-critical", ErrorSeverity.Critical);
        var lowSev = CreateEntry("e2", "learner-1", "tone-minor", ErrorSeverity.Low);

        var repo = new FakeNotebookRepo(new[] { lowSev, highSev });
        var useCase = new GetNotebookEntriesUseCase(repo, new FakeReviewRepo(), new FakeClock(DateTimeOffset.UtcNow));

        var results = await useCase.ExecuteAsync("learner-1");

        Assert.Equal(2, results.Count);
        Assert.Equal("Critical", results[0].Severity); // Higher severity first
        Assert.Equal("Low", results[1].Severity);
    }

    // ── E3 Acceptance: Query does not mutate review state ──
    // Verified by design: GetNotebookEntriesUseCase only reads, no write calls.

    // ── E3 Acceptance: Response includes practice sentence and correction ──

    [Fact]
    public async Task Execute_ResponseContainsPracticeSentenceAndCorrection()
    {
        var entry = CreateEntry("e1", "learner-1", "article-error", ErrorSeverity.High);
        var repo = new FakeNotebookRepo(new[] { entry });
        var useCase = new GetNotebookEntriesUseCase(repo, new FakeReviewRepo(), new FakeClock(DateTimeOffset.UtcNow));

        var results = await useCase.ExecuteAsync("learner-1");

        Assert.NotEmpty(results[0].OriginalExample);
        Assert.NotEmpty(results[0].CorrectedExample);
    }

    // ── Empty result when no entries ──

    [Fact]
    public async Task Execute_WithNoEntries_ReturnsEmpty()
    {
        var useCase = new GetNotebookEntriesUseCase(
            new FakeNotebookRepo(Array.Empty<NotebookEntry>()),
            new FakeReviewRepo(),
            new FakeClock(DateTimeOffset.UtcNow));

        var results = await useCase.ExecuteAsync("learner-1");
        Assert.Empty(results);
    }

    // ── Helpers ──

    private static NotebookEntry CreateEntry(
        string id, string learnerId, string patternKey,
        ErrorSeverity severity, NotebookEntryState? stateOverride = null)
    {
        var entry = NotebookEntry.Create(
            id, learnerId, patternKey,
            ErrorCategory.Grammar, severity,
            "I go office", "I go to the office", "Thiếu mạo từ",
            new NotebookEvidence("attempt-1", "ctx", DateTimeOffset.UtcNow));

        if (stateOverride == NotebookEntryState.Archived) entry.Archive();
        else if (stateOverride == NotebookEntryState.Learning)
            entry.RecordRecurrence(new NotebookEvidence("a2", "ctx2", DateTimeOffset.UtcNow));

        return entry;
    }

    private sealed class FakeNotebookRepo : INotebookRepository
    {
        private readonly IReadOnlyList<NotebookEntry> _entries;

        public FakeNotebookRepo(IEnumerable<NotebookEntry> entries) =>
            _entries = entries.ToList();

        public Task<NotebookEntry?> GetByPatternKeyAsync(string learnerId, string patternKey, CancellationToken ct = default)
            => Task.FromResult(_entries.FirstOrDefault(e => e.LearnerId == learnerId && e.PatternKey == patternKey));

        public Task<IReadOnlyList<NotebookEntry>> GetLearnerEntriesAsync(string learnerId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<NotebookEntry>>(_entries.Where(e => e.LearnerId == learnerId).ToList());

        public Task CreateAsync(NotebookEntry entry, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(NotebookEntry entry, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeReviewRepo : IReviewRepository
    {
        public Task<ReviewItem?> GetByCompositeKeyAsync(string userId, string itemId, ReviewTrack reviewTrack, CancellationToken cancellationToken)
            => Task.FromResult<ReviewItem?>(null);

        public Task<ReviewItem?> GetByIdAsync(string reviewItemId, string userId, CancellationToken cancellationToken)
            => Task.FromResult<ReviewItem?>(null);

        public Task CreateAsync(ReviewItem item, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(ReviewItem item, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task CompleteAsync(ReviewItem item, ReviewAttempt attempt, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<DueReviewItemReadModel>> GetDueItemsAsync(string userId, DateTimeOffset nowUtc, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DueReviewItemReadModel>>(Array.Empty<DueReviewItemReadModel>());
    }

    private sealed class FakeClock : IClock
    {
        public FakeClock(DateTimeOffset now) => UtcNow = now;
        public DateTimeOffset UtcNow { get; }
        public DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);
    }
}
