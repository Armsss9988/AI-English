namespace EnglishCoach.Contracts.Roleplay;

public record StartSessionRequest(
    Guid ScenarioId,
    int Difficulty
);

public record StartSessionResponse(
    Guid SessionId,
    string FirstClientMessage,
    string ScenarioTitle
);

public record CompleteTurnRequest(
    Guid SessionId,
    string LearnerMessage
);

public record CompleteTurnResponse(
    Guid SessionId,
    string ClientMessage,
    bool IsSessionComplete,
    string? CoachingNote
);

public record FinalizeSessionRequest(
    Guid SessionId
);

public record FinalizeSessionResponse(
    Guid SessionId,
    string CoachingSummary,
    int TotalTurns,
    IReadOnlyList<string> SuccessCriteria,
    IReadOnlyList<string> AreasToImprove
);

public enum RoleplaySessionStatus
{
    Created,
    Active,
    AwaitingFeedback,
    Finalized,
    Archived
}