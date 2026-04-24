namespace EnglishCoach.Contracts.Roleplay;

public record StartRoleplayRequest(string ScenarioId);

public record StartRoleplayResponse(
    Guid SessionId,
    string Status,
    string ScenarioTitle,
    string InitialMessage
);

public record RecordTurnRequest(string LearnerMessage);

public record RecordTurnResponse(
    string ClientMessage,
    string? CoachingNote,
    bool IsSessionComplete
);

public record RoleplaySessionResponse(
    Guid SessionId,
    Guid ScenarioId,
    string Status,
    IReadOnlyList<RoleplayTurnResponse> Turns,
    string? Summary,
    DateTimeOffset CreatedAt
);

public record RoleplayTurnResponse(
    string Speaker,
    string Message,
    DateTimeOffset Timestamp
);
