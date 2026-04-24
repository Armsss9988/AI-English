namespace EnglishCoach.Contracts.Speaking;

public record SubmitAttemptRequest(
    Guid SessionId,
    string AudioUrl,
    string Transcript
);

public record SubmitAttemptResponse(
    Guid AttemptId,
    string Status,
    SpeakingFeedbackResponse? Feedback
);

public record SpeakingFeedbackResponse(
    string PronunciationScore,
    string FluencyScore,
    string OverallFeedback,
    IReadOnlyList<string> AreasToImprove
);

public record SpeakingAttemptStateResponse(
    Guid AttemptId,
    Guid SessionId,
    string Status,
    string? Transcript,
    string? Feedback,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EvaluatedAt
);
