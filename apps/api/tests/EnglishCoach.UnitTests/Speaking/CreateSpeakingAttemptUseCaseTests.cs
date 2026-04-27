using EnglishCoach.Application.Curriculum;
using EnglishCoach.Application.Speaking;
using EnglishCoach.Domain.Curriculum;
using EnglishCoach.Domain.Speaking;

namespace EnglishCoach.UnitTests.Speaking;

public class CreateSpeakingAttemptUseCaseTests
{
    private readonly FakeAttemptRepository _attemptRepo = new();
    private readonly FakePhraseRepository _phraseRepo = new();

    private CreateSpeakingAttemptUseCase CreateUseCase() =>
        new(_attemptRepo, _phraseRepo);

    // ── S2 Acceptance: Use case validates user and content item ──

    [Fact]
    public async Task Execute_WithValidInputs_CreatesAttempt()
    {
        var phrase = CreatePublishedPhrase("content-1");
        _phraseRepo.Add(phrase);

        var useCase = CreateUseCase();
        var attemptId = await useCase.ExecuteAsync("learner-1", "content-1", null);

        Assert.NotEqual(Guid.Empty, attemptId);
        Assert.Single(_attemptRepo.Created);
    }

    [Fact]
    public async Task Execute_WithEmptyLearnerId_Throws()
    {
        var useCase = CreateUseCase();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync("", "content-1", null));
    }

    [Fact]
    public async Task Execute_WithMissingContentItem_Throws()
    {
        var useCase = CreateUseCase();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync("learner-1", "nonexistent", null));
    }

    // ── S2 Acceptance: Attempt starts in Created ──

    [Fact]
    public async Task Execute_WithoutTranscript_AttemptIsInCreatedState()
    {
        var phrase = CreatePublishedPhrase("content-1");
        _phraseRepo.Add(phrase);

        var useCase = CreateUseCase();
        await useCase.ExecuteAsync("learner-1", "content-1", null);

        var created = _attemptRepo.Created[0];
        Assert.Equal(SpeakingAttemptState.Created, created.State);
    }

    // ── S2 Acceptance: MVP supports text transcript input ──

    [Fact]
    public async Task Execute_WithInitialTranscript_SetsTranscribedState()
    {
        var phrase = CreatePublishedPhrase("content-1");
        _phraseRepo.Add(phrase);

        var useCase = CreateUseCase();
        await useCase.ExecuteAsync("learner-1", "content-1", "This is my spoken text");

        var created = _attemptRepo.Created[0];
        Assert.Equal(SpeakingAttemptState.Transcribed, created.State);
    }

    // ── S2 Acceptance: Controller calls use case only ──
    // Verified structurally: Program.cs endpoint delegates to use case.

    // ── Helpers ──

    private static Phrase CreatePublishedPhrase(string id)
    {
        var phrase = Phrase.Create(id, "test", "test meaning",
            CommunicationFunction.Standup, ContentLevel.Core, "example");
        phrase.SubmitForReview();
        phrase.Publish();
        return phrase;
    }

    private sealed class FakeAttemptRepository : ISpeakingAttemptRepository
    {
        public List<SpeakingAttempt> Created { get; } = new();

        public Task<SpeakingAttempt?> GetByIdAsync(string attemptId, CancellationToken ct = default)
            => Task.FromResult(Created.FirstOrDefault(a => a.Id == attemptId));

        public Task CreateAsync(SpeakingAttempt attempt, CancellationToken ct = default)
        {
            Created.Add(attempt);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(SpeakingAttempt attempt, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakePhraseRepository : IPhraseRepository
    {
        private readonly List<Phrase> _phrases = new();

        public void Add(Phrase phrase) => _phrases.Add(phrase);

        public Task<Phrase?> GetByIdAsync(string phraseId, CancellationToken ct = default)
            => Task.FromResult(_phrases.FirstOrDefault(p => p.Id == phraseId));

        public Task<IReadOnlyList<Phrase>> GetPublishedByFunctionAndLevelAsync(
            CommunicationFunction? function, ContentLevel? level, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Phrase>>(_phrases.Where(p => p.IsPublished).ToList());

        public Task<IReadOnlyList<Phrase>> GetAllPublishedAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Phrase>>(_phrases.Where(p => p.IsPublished).ToList());

        public Task<IReadOnlyList<Phrase>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Phrase>>(_phrases.ToList());

        public Task CreateAsync(Phrase phrase, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(Phrase phrase, CancellationToken ct = default) => Task.CompletedTask;
    }
}
