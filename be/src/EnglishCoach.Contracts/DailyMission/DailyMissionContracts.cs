namespace EnglishCoach.Contracts.DailyMission;

public record GetDailyMissionRequest(Guid LearnerId);

public record GetDailyMissionResponse(
    Guid LearnerId,
    string MissionDate,
    DailyMissionSectionContract Reviews,
    DailyMissionSectionContract Speaking,
    DailyMissionSectionContract Roleplay,
    DailyMissionSectionContract Retry,
    int TotalItems,
    bool IsComplete
);

public record DailyMissionSectionContract(
    string SectionName,
    int Required,
    int Provided,
    IReadOnlyList<DailyMissionItemContract> Items,
    bool IsDegraded
);

public record DailyMissionItemContract(
    Guid Id,
    string Title,
    string? Subtitle,
    string Category,
    string ItemType
);