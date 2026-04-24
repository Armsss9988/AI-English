namespace EnglishCoach.Domain.DailyMission;

public record DailyMissionPolicy(
    int DueReviewCount = 5,
    int SpeakingDrillCount = 1,
    int RoleplayScenarioCount = 1,
    int RetryTaskCount = 1,
    int CriticalErrorThreshold = 1,
    int RecentDaysForCriticalError = 7
);

public static class DailyMissionPolicies
{
    public static readonly DailyMissionPolicy Default = new();
}
