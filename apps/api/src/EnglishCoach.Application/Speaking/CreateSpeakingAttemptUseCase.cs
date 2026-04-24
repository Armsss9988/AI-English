using EnglishCoach.Domain.Speaking;
using EnglishCoach.Application.Curriculum;

namespace EnglishCoach.Application.Speaking;

public interface ISpeakingAttemptRepository
{
    Task<SpeakingAttempt?> GetByIdAsync(string attemptId, CancellationToken ct = default);
    Task CreateAsync(SpeakingAttempt attempt, CancellationToken ct = default);
    Task UpdateAsync(SpeakingAttempt attempt, CancellationToken ct = default);
}

public sealed class CreateSpeakingAttemptUseCase
{
    private readonly ISpeakingAttemptRepository _attemptRepository;
    private readonly IPhraseRepository _phraseRepository;

    public CreateSpeakingAttemptUseCase(
        ISpeakingAttemptRepository attemptRepository,
        IPhraseRepository phraseRepository)
    {
        _attemptRepository = attemptRepository;
        _phraseRepository = phraseRepository;
    }

    public async Task<Guid> ExecuteAsync(
        string learnerId,
        string contentItemId,
        string? initialTranscript, // MVP supports text input
        CancellationToken ct = default)
    {
        // Use case validates user and content item.
        if (string.IsNullOrWhiteSpace(learnerId))
            throw new ArgumentException("Learner ID is required.");

        var contentItem = await _phraseRepository.GetByIdAsync(contentItemId, ct);
        if (contentItem is null)
        {
            throw new InvalidOperationException("Content item not found.");
        }

        var attemptId = Guid.NewGuid().ToString("N");
        var attempt = SpeakingAttempt.Create(attemptId, learnerId, contentItemId);

        // MVP supports text transcript input directly without audio upload
        if (!string.IsNullOrWhiteSpace(initialTranscript))
        {
            // Set raw and normalized to same for MVP text input
            attempt.MarkTranscribed(initialTranscript, initialTranscript);
        }

        await _attemptRepository.CreateAsync(attempt, ct);

        return Guid.Parse(attemptId);
    }
}
