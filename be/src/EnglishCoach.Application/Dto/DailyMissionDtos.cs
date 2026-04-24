namespace EnglishCoach.Application.Dto;

public record DailyMissionDto(
    Guid LearnerId,
    DateOnly MissionDate,
    DailyMissionSectionDto Reviews,
    DailyMissionSectionDto Speaking,
    DailyMissionSectionDto Roleplay,
    DailyMissionSectionDto Retry,
    int TotalItems,
    bool IsComplete
);

public record DailyMissionSectionDto(
    string SectionName,
    int Required,
    int Provided,
    IReadOnlyList<DailyMissionItemDto> Items,
    bool IsDegraded
);

public record DailyMissionItemDto(
    Guid Id,
    string Title,
    string? Subtitle,
    string Category,
    string ItemType
);