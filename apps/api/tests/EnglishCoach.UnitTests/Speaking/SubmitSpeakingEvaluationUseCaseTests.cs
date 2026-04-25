using EnglishCoach.Application.Ports;
using EnglishCoach.Application.Speaking;
using EnglishCoach.Domain.Speaking;

namespace EnglishCoach.UnitTests.Speaking;

public class SubmitSpeakingEvaluationUseCaseTests
{
    // ── S3 Acceptance: Must be transcribed before evaluation ──

    [Fact]
    public async Task Execute_WhenAttemptIsTranscribed_ReturnsValidFeedback()
    {
        var attemptId = Guid.NewGuid().ToString("N");
        var attempt = SpeakingAttempt.Create(attemptId, "learner-1", "content-1");
        attempt.MarkTranscribed("I will check with the team", "I will check with the team");

        var repo = new FakeAttemptRepo(attempt);
        var feedbackService = new FakeFeedbackService();
        var useCase = new SubmitSpeakingAttemptEvaluationUseCase(repo, feedbackService);

        var result = await useCase.ExecuteAsync("learner-1", Guid.Parse(attemptId));

        Assert.NotNull(result);
        Assert.NotEmpty(result.TopMistakes);
    }

    // ── S3 Acceptance: Non-owner cannot evaluate ──

    [Fact]
    public async Task Execute_WhenNotOwner_Throws()
    {
        var attemptId = Guid.NewGuid().ToString("N");
        var attempt = SpeakingAttempt.Create(attemptId, "learner-1", "content-1");
        attempt.MarkTranscribed("text", "text");

        var repo = new FakeAttemptRepo(attempt);
        var useCase = new SubmitSpeakingAttemptEvaluationUseCase(repo, new FakeFeedbackService());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync("other-user", Guid.Parse(attemptId)));
    }

    // ── S3 Acceptance: Attempt not found throws ──

    [Fact]
    public async Task Execute_WhenAttemptNotFound_Throws()
    {
        var repo = new FakeAttemptRepo(null);
        var useCase = new SubmitSpeakingAttemptEvaluationUseCase(repo, new FakeFeedbackService());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync("learner-1", Guid.NewGuid()));
    }

    // ── S3 Acceptance: Attempt must be transcribed ──

    [Fact]
    public async Task Execute_WhenAttemptIsCreated_Throws()
    {
        var attemptId = Guid.NewGuid().ToString("N");
        var attempt = SpeakingAttempt.Create(attemptId, "learner-1", "content-1");

        var repo = new FakeAttemptRepo(attempt);
        var useCase = new SubmitSpeakingAttemptEvaluationUseCase(repo, new FakeFeedbackService());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync("learner-1", Guid.Parse(attemptId)));
    }

    // ── S3 Acceptance: Feedback includes top mistakes, improved answer, phrases to review, retry prompt ──

    [Fact]
    public async Task Execute_FeedbackContainsAllFields()
    {
        var attemptId = Guid.NewGuid().ToString("N");
        var attempt = SpeakingAttempt.Create(attemptId, "learner-1", "content-1");
        attempt.MarkTranscribed("I check team", "I check team");

        var repo = new FakeAttemptRepo(attempt);
        var useCase = new SubmitSpeakingAttemptEvaluationUseCase(repo, new FakeFeedbackService());

        var result = await useCase.ExecuteAsync("learner-1", Guid.Parse(attemptId));

        Assert.NotNull(result.TopMistakes);
        Assert.NotNull(result.PhrasesToReview);
        Assert.NotNull(result.RetryPrompt);
    }

    // ── Fakes ──

    private sealed class FakeAttemptRepo : ISpeakingAttemptRepository
    {
        private readonly SpeakingAttempt? _stored;
        private SpeakingAttempt? _updated;

        public FakeAttemptRepo(SpeakingAttempt? stored) => _stored = stored;

        public Task<SpeakingAttempt?> GetByIdAsync(string attemptId, CancellationToken ct = default)
            => Task.FromResult(_stored?.Id == attemptId ? _stored : null);

        public Task CreateAsync(SpeakingAttempt attempt, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task UpdateAsync(SpeakingAttempt attempt, CancellationToken ct = default)
        {
            _updated = attempt;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeFeedbackService : ISpeakingFeedbackService
    {
        public ProviderKind Provider => ProviderKind.Fake;

        public Task<FeedbackResult> GenerateFeedbackAsync(
            SpeakingAttemptForEvaluation attempt, CancellationToken ct = default)
        {
            return Task.FromResult(FeedbackResult.Success(new SpeakingFeedbackContent
            {
                PronunciationScore = "85",
                FluencyScore = "70",
                OverallFeedback = "Try to use more natural phrases.",
                AreasToImprove = new[] { "tense agreement", "article usage" },
                Strengths = new[] { "good vocabulary", "clear message" }
            }, ProviderKind.Fake));
        }
    }
}
