namespace EnglishCoach.Application.Ports;

public enum ProviderKind
{
    OpenAI,
    Azure,
    Anthropic,
    Fake
}

public interface ISpeechTranscriptionService
{
    Task<TranscriptionResult> TranscribeAsync(AudioReference audio, CancellationToken ct = default);
    ProviderKind Provider { get; }
}

public record AudioReference(Guid AttemptId, string AudioUrl);

public record TranscriptionResult
{
    public bool IsSuccess { get; init; }
    public string? Transcript { get; init; }
    public string? ErrorMessage { get; init; }
    public string ErrorCode { get; init; } = string.Empty;
    public ProviderKind Provider { get; init; }
    public DateTimeOffset CompletedAt { get; init; }

    public static TranscriptionResult Success(string transcript, ProviderKind provider) => new()
    {
        IsSuccess = true,
        Transcript = transcript,
        Provider = provider,
        CompletedAt = DateTimeOffset.UtcNow
    };

    public static TranscriptionResult Failure(string errorCode, string errorMessage, ProviderKind provider) => new()
    {
        IsSuccess = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        Provider = provider,
        CompletedAt = DateTimeOffset.UtcNow
    };
}

public interface ISpeakingFeedbackService
{
    Task<FeedbackResult> GenerateFeedbackAsync(
        SpeakingAttemptForEvaluation attempt,
        CancellationToken ct = default);

    ProviderKind Provider { get; }
}

public record SpeakingAttemptForEvaluation(
    Guid AttemptId,
    Guid SessionId,
    string Transcript
);

public record FeedbackResult
{
    public bool IsSuccess { get; init; }
    public SpeakingFeedbackContent? Content { get; init; }
    public string? ErrorMessage { get; init; }
    public string ErrorCode { get; init; } = string.Empty;
    public ProviderKind Provider { get; init; }
    public DateTimeOffset CompletedAt { get; init; }

    public static FeedbackResult Success(SpeakingFeedbackContent content, ProviderKind provider) => new()
    {
        IsSuccess = true,
        Content = content,
        Provider = provider,
        CompletedAt = DateTimeOffset.UtcNow
    };

    public static FeedbackResult Failure(string errorCode, string errorMessage, ProviderKind provider) => new()
    {
        IsSuccess = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        Provider = provider,
        CompletedAt = DateTimeOffset.UtcNow
    };
}

public record SpeakingFeedbackContent
{
    public string PronunciationScore { get; init; } = string.Empty;
    public string FluencyScore { get; init; } = string.Empty;
    public string OverallFeedback { get; init; } = string.Empty;
    public IReadOnlyList<string> AreasToImprove { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Strengths { get; init; } = Array.Empty<string>();
}

public interface IRoleplayResponseService
{
    Task<RoleplayResult> GenerateResponseAsync(
        RoleplayContext context,
        CancellationToken ct = default);

    ProviderKind Provider { get; }
}

public record RoleplayContext
{
    public Guid SessionId { get; init; }
    public Guid ScenarioId { get; init; }
    public string ScenarioTitle { get; init; } = string.Empty;
    public string ScenarioPersona { get; init; } = string.Empty;
    public string ScenarioGoal { get; init; } = string.Empty;
    public IReadOnlyList<RoleplayTurn> ConversationHistory { get; init; } = Array.Empty<RoleplayTurn>();
    public RoleplayTurn? LatestLearnerTurn { get; init; }
    public int Difficulty { get; init; }
    public IReadOnlyList<string> SuccessCriteria { get; init; } = Array.Empty<string>();
}

public record RoleplayTurn
{
    public string Speaker { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
}

public record RoleplayResult
{
    public bool IsSuccess { get; init; }
    public RoleplayResponseContent? Content { get; init; }
    public string? ErrorMessage { get; init; }
    public string ErrorCode { get; init; } = string.Empty;
    public ProviderKind Provider { get; init; }
    public DateTimeOffset CompletedAt { get; init; }

    public static RoleplayResult Success(RoleplayResponseContent content, ProviderKind provider) => new()
    {
        IsSuccess = true,
        Content = content,
        Provider = provider,
        CompletedAt = DateTimeOffset.UtcNow
    };

    public static RoleplayResult Failure(string errorCode, string errorMessage, ProviderKind provider) => new()
    {
        IsSuccess = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        Provider = provider,
        CompletedAt = DateTimeOffset.UtcNow
    };
}

public record RoleplayResponseContent
{
    public string ClientMessage { get; init; } = string.Empty;
    public string? CoachingNote { get; init; }
    public bool IsSessionComplete { get; init; }
    public IReadOnlyList<string> EvaluatedCriteria { get; init; } = Array.Empty<string>();
}