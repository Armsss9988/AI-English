namespace EnglishCoach.Contracts.DailyMission;

public record DailyMissionResponse(
    DateOnly MissionDate,
    IReadOnlyList<MissionTaskResponse> Missions,
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
