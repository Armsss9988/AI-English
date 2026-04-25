using EnglishCoach.Application.ErrorNotebook;
using EnglishCoach.Domain.ErrorNotebook;

namespace EnglishCoach.UnitTests.ErrorNotebook;

public class PromoteErrorPatternUseCaseTests
{
    // ── E2 Acceptance: New pattern creates entry ──

    [Fact]
    public async Task Execute_NewPattern_CreatesEntry()
    {
        var repo = new FakeNotebookRepo();
        var reviewService = new FakeReviewIntegration();
        var useCase = new PromoteErrorPatternUseCase(repo, reviewService);

        var request = new PromoteErrorPatternRequest(
            "missing-article", ErrorCategory.Grammar, ErrorSeverity.High,
            "I go office", "I go to the office", "Thiếu mạo từ",
            "attempt-1", "Speaking drill");

        var entryId = await useCase.ExecuteAsync("learner-1", request);

        Assert.NotEmpty(entryId);
        Assert.Single(repo.Created);
        Assert.Equal("missing-article", repo.Created[0].PatternKey);
    }

    // ── E2 Acceptance: Existing pattern increments recurrence and appends evidence ──

    [Fact]
    public async Task Execute_ExistingPattern_IncrementsAndAppends()
    {
        var existing = NotebookEntry.Create(
            "entry-1", "learner-1", "missing-article",
            ErrorCategory.Grammar, ErrorSeverity.High,
            "I go office", "I go to the office", "Thiếu mạo từ",
            new NotebookEvidence("attempt-1", "First time", DateTimeOffset.UtcNow));

        var repo = new FakeNotebookRepo(existing);
        var reviewService = new FakeReviewIntegration();
        var useCase = new PromoteErrorPatternUseCase(repo, reviewService);

        var request = new PromoteErrorPatternRequest(
            "missing-article", ErrorCategory.Grammar, ErrorSeverity.High,
            "He go school", "He goes to school", "Thiếu mạo từ",
            "attempt-2", "Second occurrence");

        await useCase.ExecuteAsync("learner-1", request);

        Assert.Equal(2, existing.RecurrenceCount);
        Assert.Equal(2, existing.EvidenceRefs.Count);
        Assert.True(repo.UpdateCalled);
    }

    // ── E2 Acceptance: Use case can request EnsureReviewItemExists ──

    [Fact]
    public async Task Execute_CallsReviewIntegration()
    {
        var repo = new FakeNotebookRepo();
        var reviewService = new FakeReviewIntegration();
        var useCase = new PromoteErrorPatternUseCase(repo, reviewService);

        var request = new PromoteErrorPatternRequest(
            "tense-error", ErrorCategory.Grammar, ErrorSeverity.Medium,
            "I go yesterday", "I went yesterday", "Sai thì",
            "attempt-1", "ctx");

        await useCase.ExecuteAsync("learner-1", request);

        Assert.True(reviewService.Called);
        Assert.Equal("tense-error", reviewService.LastPatternKey);
    }

    // ── E2 Acceptance: Empty learner ID throws ──

    [Fact]
    public async Task Execute_WithEmptyLearnerId_Throws()
    {
        var useCase = new PromoteErrorPatternUseCase(new FakeNotebookRepo(), new FakeReviewIntegration());

        var request = new PromoteErrorPatternRequest(
            "key", ErrorCategory.Grammar, ErrorSeverity.Low,
            "o", "c", "e", "a1", "ctx");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync("", request));
    }

    // ── Fakes ──

    private sealed class FakeNotebookRepo : INotebookRepository
    {
        private readonly NotebookEntry? _existing;
        public List<NotebookEntry> Created { get; } = new();
        public bool UpdateCalled { get; private set; }

        public FakeNotebookRepo(NotebookEntry? existing = null) => _existing = existing;

        public Task<NotebookEntry?> GetByPatternKeyAsync(string learnerId, string patternKey, CancellationToken ct = default)
            => Task.FromResult(_existing?.PatternKey == patternKey && _existing?.LearnerId == learnerId ? _existing : null);

        public Task<IReadOnlyList<NotebookEntry>> GetLearnerEntriesAsync(string learnerId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<NotebookEntry>>(
                _existing is not null ? new[] { _existing } : Array.Empty<NotebookEntry>());

        public Task CreateAsync(NotebookEntry entry, CancellationToken ct = default)
        {
            Created.Add(entry);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(NotebookEntry entry, CancellationToken ct = default)
        {
            UpdateCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeReviewIntegration : IReviewIntegrationService
    {
        public bool Called { get; private set; }
        public string? LastPatternKey { get; private set; }

        public Task EnsureReviewItemExistsAsync(string learnerId, string patternKey, CancellationToken ct = default)
        {
            Called = true;
            LastPatternKey = patternKey;
            return Task.CompletedTask;
        }
    }
}
