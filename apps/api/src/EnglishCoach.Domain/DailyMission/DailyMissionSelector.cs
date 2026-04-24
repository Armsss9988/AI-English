using EnglishCoach.SharedKernel.Time;

namespace EnglishCoach.Domain.DailyMission;

public interface IDailyMissionDataProvider
{
    Task<IReadOnlyList<DueReviewItem>> GetDueReviewsAsync(Guid learnerId, int limit, CancellationToken ct = default);
    Task<IReadOnlyList<SpeakingTask>> GetSpeakingDrillsAsync(int limit, CancellationToken ct = default);
    Task<IReadOnlyList<RoleplayTask>> GetRoleplayScenariosAsync(string? excludeGroup, int limit, CancellationToken ct = default);
    Task<int> GetCriticalErrorCountAsync(Guid learnerId, int recentDays, CancellationToken ct = default);
    Task<IReadOnlyList<RetryTask>> GetRecentCriticalErrorsAsync(Guid learnerId, int limit, CancellationToken ct = default);
}

public class DailyMissionSelector
{
    private readonly IDailyMissionDataProvider _dataProvider;
    private readonly IClock _clock;

    public DailyMissionSelector(IDailyMissionDataProvider dataProvider, IClock clock)
    {
        _dataProvider = dataProvider;
        _clock = clock;
    }

    public async Task<DailyMissionSelection> SelectAsync(
        Guid learnerId,
        DailyMissionPolicy policy,
        CancellationToken ct = default)
    {
        var missionDate = _clock.Today;

        var dueReviews = await _dataProvider.GetDueReviewsAsync(learnerId, policy.DueReviewCount, ct);
        var speakingDrills = await _dataProvider.GetSpeakingDrillsAsync(policy.SpeakingDrillCount, ct);
        var roleplayScenarios = await _dataProvider.GetRoleplayScenariosAsync(null, policy.RoleplayScenarioCount, ct);

        IReadOnlyList<RetryTask> retryTasks = Array.Empty<RetryTask>();
        var hasRetryTask = false;

        var criticalErrorCount = await _dataProvider.GetCriticalErrorCountAsync(
            learnerId, policy.RecentDaysForCriticalError, ct);

        if (criticalErrorCount >= policy.CriticalErrorThreshold)
        {
            retryTasks = await _dataProvider.GetRecentCriticalErrorsAsync(
                learnerId, policy.RetryTaskCount, ct);
            hasRetryTask = retryTasks.Count > 0;
        }

        return new DailyMissionSelection(
            learnerId,
            missionDate,
            dueReviews,
            speakingDrills,
            roleplayScenarios,
            retryTasks,
            hasRetryTask
        );
    }
}
