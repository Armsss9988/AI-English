using EnglishCoach.Contracts.DailyMission;
using EnglishCoach.Domain.DailyMission;

namespace EnglishCoach.Application.DailyMission;

public class GetDailyMissionQuery
{
    private readonly DailyMissionSelector _selector;

    public GetDailyMissionQuery(DailyMissionSelector selector)
    {
        _selector = selector;
    }

    public async Task<DailyMissionResponse> ExecuteAsync(Guid learnerId, CancellationToken ct = default)
    {
        var selection = await _selector.SelectAsync(learnerId, DailyMissionPolicies.Default, ct);

        var tasks = new List<MissionTaskResponse>();

        foreach (var review in selection.DueReviews)
        {
            tasks.Add(new MissionTaskResponse(
                "Review",
                review.ReviewItemId,
                review.PhraseText,
                "Vocabulary",
                review.Category
            ));
        }

        foreach (var drill in selection.SpeakingDrills)
        {
            tasks.Add(new MissionTaskResponse(
                "Speaking",
                drill.ContentItemId,
                drill.Title,
                drill.Category,
                drill.ContentType.ToString()
            ));
        }

        foreach (var scenario in selection.RoleplayScenarios)
        {
            tasks.Add(new MissionTaskResponse(
                "Roleplay",
                scenario.ScenarioId,
                scenario.Title,
                scenario.ScenarioGroup,
                scenario.Goal
            ));
        }

        foreach (var retry in selection.RetryTasks)
        {
            tasks.Add(new MissionTaskResponse(
                "Retry",
                retry.NotebookEntryId,
                retry.ErrorPattern,
                retry.Category,
                retry.CorrectedForm
            ));
        }

        return new DailyMissionResponse(
            selection.MissionDate,
            tasks,
            tasks.Count,
            selection.HasRetryTask
        );
    }
}
