namespace EnglishCoach.Domain.Progress;

public enum CapabilityName
{
    CanGiveDailyUpdate,
    CanExplainBug,
    CanAskClarification,
    CanReportDelay,
    CanProposeOptions,
    CanSummarizeNextSteps
}

public record CapabilityCriteria(
    CapabilityName Name,
    int RequiredRoleplayScenarios,
    int RequiredPhraseMastery,
    int MaxCriticalErrors,
    string Description
);

public static class CanDoCriteria
{
    public static IReadOnlyList<CapabilityCriteria> All => new[]
    {
        new CapabilityCriteria(
            CapabilityName.CanGiveDailyUpdate,
            RequiredRoleplayScenarios: 3,
            RequiredPhraseMastery: 2,
            MaxCriticalErrors: 3,
            "Able to give clear daily standup updates covering progress, plans, and blockers"
        ),
        new CapabilityCriteria(
            CapabilityName.CanExplainBug,
            RequiredRoleplayScenarios: 3,
            RequiredPhraseMastery: 3,
            MaxCriticalErrors: 2,
            "Able to clearly explain technical issues to non-technical stakeholders"
        ),
        new CapabilityCriteria(
            CapabilityName.CanAskClarification,
            RequiredRoleplayScenarios: 2,
            RequiredPhraseMastery: 2,
            MaxCriticalErrors: 4,
            "Able to ask targeted clarifying questions when requirements are unclear"
        ),
        new CapabilityCriteria(
            CapabilityName.CanReportDelay,
            RequiredRoleplayScenarios: 2,
            RequiredPhraseMastery: 2,
            MaxCriticalErrors: 3,
            "Able to professionally communicate timeline adjustments to stakeholders"
        ),
        new CapabilityCriteria(
            CapabilityName.CanProposeOptions,
            RequiredRoleplayScenarios: 3,
            RequiredPhraseMastery: 3,
            MaxCriticalErrors: 2,
            "Able to present multiple solution options with trade-offs"
        ),
        new CapabilityCriteria(
            CapabilityName.CanSummarizeNextSteps,
            RequiredRoleplayScenarios: 3,
            RequiredPhraseMastery: 2,
            MaxCriticalErrors: 3,
            "Able to concisely summarize decisions and action items"
        )
    };

    public static CapabilityCriteria Get(CapabilityName name) =>
        All.First(c => c.Name == name);
}