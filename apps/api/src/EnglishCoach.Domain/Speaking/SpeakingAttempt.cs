namespace EnglishCoach.Domain.Speaking;

public sealed record SpeakingFeedback(
    string TopMistakes,
    string ImprovedAnswer,
    string PhrasesToReview,
    string RetryPrompt
);

public sealed class SpeakingAttempt
{
    private SpeakingAttempt()
    {
        Id = string.Empty;
        LearnerId = string.Empty;
        ContentItemId = string.Empty;
        RawTranscript = string.Empty;
        NormalizedTranscript = string.Empty;
        AudioUrl = string.Empty;
    }

    public string Id { get; private set; }
    public string LearnerId { get; private set; }
    public string ContentItemId { get; private set; }
    public SpeakingAttemptState State { get; private set; }
    
    public string AudioUrl { get; private set; }
    public string RawTranscript { get; private set; }
    public string NormalizedTranscript { get; private set; }
    
    public SpeakingFeedback? Feedback { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SpeakingAttempt Create(string id, string learnerId, string contentItemId)
    {
        return new SpeakingAttempt
        {
            Id = RequireNonEmpty(id, nameof(id)),
            LearnerId = RequireNonEmpty(learnerId, nameof(learnerId)),
            ContentItemId = RequireNonEmpty(contentItemId, nameof(contentItemId)),
            State = SpeakingAttemptState.Created,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public void MarkUploaded(string audioUrl)
    {
        if (State != SpeakingAttemptState.Created)
            throw new InvalidOperationException($"Cannot transition from {State} to Uploaded.");
            
        AudioUrl = RequireNonEmpty(audioUrl, nameof(audioUrl));
        State = SpeakingAttemptState.Uploaded;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkTranscribed(string rawTranscript, string normalizedTranscript)
    {
        // Valid from Created (if MVP skips upload) or Uploaded
        if (State != SpeakingAttemptState.Created && State != SpeakingAttemptState.Uploaded)
            throw new InvalidOperationException($"Cannot transition from {State} to Transcribed.");

        RawTranscript = rawTranscript;
        NormalizedTranscript = normalizedTranscript;
        State = SpeakingAttemptState.Transcribed;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkEvaluated(SpeakingFeedback feedback)
    {
        if (State != SpeakingAttemptState.Transcribed)
            throw new InvalidOperationException($"Cannot transition from {State} to Evaluated.");

        Feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
        State = SpeakingAttemptState.Evaluated;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void FinalizeAttempt()
    {
        if (State != SpeakingAttemptState.Evaluated)
            throw new InvalidOperationException($"Cannot transition from {State} to Finalized.");

        State = SpeakingAttemptState.Finalized;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string RequireNonEmpty(string value, string paramName) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", paramName) : value.Trim();
}
