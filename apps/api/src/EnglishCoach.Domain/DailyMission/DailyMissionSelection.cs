using EnglishCoach.Domain.LearningContent;

namespace EnglishCoach.Domain.DailyMission;

public record DueReviewItem(
    Guid ReviewItemId,
    Guid PhraseId,
    string PhraseText,
    string Category,
    DateTimeOffset DueAt
);

public record SpeakingTask(
    Guid ContentItemId,
    string Title,
    string Category,
    ContentType ContentType
);

public record RoleplayTask(
    Guid ScenarioId,
    string Title,
    string Goal,
    string Persona,
    string ScenarioGroup
);

public record RetryTask(
    Guid NotebookEntryId,
    string ErrorPattern,
    string CorrectedForm,
    string Category
);

public record DailyMissionSelection(
    Guid LearnerId,
    DateOnly MissionDate,
    IReadOnlyList<DueReviewItem> DueReviews,
    IReadOnlyList<SpeakingTask> SpeakingDrills,
    IReadOnlyList<RoleplayTask> RoleplayScenarios,
    IReadOnlyList<RetryTask> RetryTasks,
    bool HasRetryTask
)
{
    public int TotalItems =>
        DueReviews.Count + SpeakingDrills.Count + RoleplayScenarios.Count + RetryTasks.Count;

    public bool IsComplete =>
        DueReviews.Count > 0 || SpeakingDrills.Count > 0 || RoleplayScenarios.Count > 0;
}
