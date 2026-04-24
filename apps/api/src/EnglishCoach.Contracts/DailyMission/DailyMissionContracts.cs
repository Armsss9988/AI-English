namespace EnglishCoach.Contracts.DailyMission;

public record DailyMissionResponse(
    DateOnly MissionDate,
    IReadOnlyList<MissionTaskResponse> Tasks,
    int TotalItems,
    bool HasRetryTask
);

public record MissionTaskResponse(
    string Type,
    Guid ItemId,
    string Title,
    string Category,
    string? Description
);
