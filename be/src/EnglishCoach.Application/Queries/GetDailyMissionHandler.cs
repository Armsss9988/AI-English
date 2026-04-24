using EnglishCoach.Application.Dto;
using EnglishCoach.Domain.DailyMission;
using EnglishCoach.SharedKernel.Clock;
using EnglishCoach.SharedKernel.Result;

namespace EnglishCoach.Application.Queries;

public class GetDailyMissionHandler : IGetDailyMissionHandler
{
    private readonly DailyMissionSelector _selector;
    private readonly DailyMissionPolicy _policy;
    private readonly IClock _clock;

    public GetDailyMissionHandler(
        DailyMissionSelector selector,
        DailyMissionPolicy policy,
        IClock clock)
    {
        _selector = selector;
        _policy = policy;
        _clock = clock;
    }

    public async Task<DailyMissionDto> HandleAsync(
        GetDailyMissionQuery query,
        CancellationToken ct = default)
    {
        DailyMissionSelection selection;

        try
        {
            selection = await _selector.SelectAsync(query.LearnerId, _policy, ct);
        }
        catch
        {
            selection = _selector.SelectWithGracefulDegradation(
                query.LearnerId, _policy, new FallbackDataProvider());
        }

        return MapToDto(selection);
    }

    private DailyMissionDto MapToDto(DailyMissionSelection selection)
    {
        return new DailyMissionDto(
            selection.LearnerId,
            selection.MissionDate,
            MapReviewsSection(selection),
            MapSpeakingSection(selection),
            MapRoleplaySection(selection),
            MapRetrySection(selection),
            selection.TotalItems,
            selection.IsComplete
        );
    }

    private DailyMissionSectionDto MapReviewsSection(DailyMissionSelection selection)
    {
        var items = selection.DueReviews.Select(r => new DailyMissionItemDto(
            r.ReviewItemId,
            r.PhraseText,
            $"Due: {r.DueAt:g}",
            r.Category,
            "review"
        )).ToList();

        return new DailyMissionSectionDto(
            "Due Reviews",
            _policy.DueReviewCount,
            selection.DueReviews.Count,
            items,
            selection.DueReviews.Count < _policy.DueReviewCount
        );
    }

    private DailyMissionSectionDto MapSpeakingSection(DailyMissionSelection selection)
    {
        var items = selection.SpeakingDrills.Select(s => new DailyMissionItemDto(
            s.ContentItemId,
            s.Title,
            s.ContentType.ToString(),
            s.Category,
            "speaking"
        )).ToList();

        return new DailyMissionSectionDto(
            "Speaking Drills",
            _policy.SpeakingDrillCount,
            selection.SpeakingDrills.Count,
            items,
            selection.SpeakingDrills.Count < _policy.SpeakingDrillCount
        );
    }

    private DailyMissionSectionDto MapRoleplaySection(DailyMissionSelection selection)
    {
        var items = selection.RoleplayScenarios.Select(r => new DailyMissionItemDto(
            r.ScenarioId,
            r.Title,
            r.Persona,
            r.ScenarioGroup,
            "roleplay"
        )).ToList();

        return new DailyMissionSectionDto(
            "Roleplay Scenarios",
            _policy.RoleplayScenarioCount,
            selection.RoleplayScenarios.Count,
            items,
            selection.RoleplayScenarios.Count < _policy.RoleplayScenarioCount
        );
    }

    private DailyMissionSectionDto MapRetrySection(DailyMissionSelection selection)
    {
        var items = selection.RetryTasks.Select(r => new DailyMissionItemDto(
            r.NotebookEntryId,
            r.ErrorPattern,
            r.CorrectedForm,
            r.Category,
            "retry"
        )).ToList();

        return new DailyMissionSectionDto(
            "Retry Tasks",
            selection.HasRetryTask ? _policy.RetryTaskCount : 0,
            selection.RetryTasks.Count,
            items,
            selection.HasRetryTask && selection.RetryTasks.Count == 0
        );
    }

    private class FallbackDataProvider : IDailyMissionDataProvider
    {
        public Task<IReadOnlyList<DueReviewItem>> GetDueReviewsAsync(Guid learnerId, int limit, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<DueReviewItem>>(Array.Empty<DueReviewItem>());

        public Task<IReadOnlyList<SpeakingTask>> GetSpeakingDrillsAsync(int limit, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<SpeakingTask>>(Array.Empty<SpeakingTask>());

        public Task<IReadOnlyList<RoleplayTask>> GetRoleplayScenariosAsync(string? excludeGroup, int limit, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<RoleplayTask>>(Array.Empty<RoleplayTask>());

        public Task<int> GetCriticalErrorCountAsync(Guid learnerId, int recentDays, CancellationToken ct = default)
            => Task.FromResult(0);

        public Task<IReadOnlyList<RetryTask>> GetRecentCriticalErrorsAsync(Guid learnerId, int limit, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<RetryTask>>(Array.Empty<RetryTask>());
    }
}