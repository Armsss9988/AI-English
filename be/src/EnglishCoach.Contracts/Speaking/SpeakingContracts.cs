using EnglishCoach.SharedKernel.Ids;

namespace EnglishCoach.Contracts.Speaking;

public record SubmitAttemptRequest(
    Guid SessionId,
    string AudioUrl,
    string Transcript
);

public record SubmitAttemptResponse(
    Guid AttemptId,
    string Status,
    SpeakingFeedback? Feedback
);

public record SpeakingFeedback(
    string PronunciationScore,
    string FluencyScore,
    string OverallFeedback,
    IReadOnlyList<string> AreasToImprove
);

public record SpeakingAttemptState(
    Guid AttemptId,
    Guid SessionId,
    string Status,
    string? Transcript,
    string? Feedback,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EvaluatedAt
);