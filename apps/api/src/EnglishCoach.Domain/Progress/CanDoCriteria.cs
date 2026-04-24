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
            CapabilityName.CanGiveDailyUpdate, 3, 2, 3,
            "Able to give clear daily standup updates covering progress, plans, and blockers"),
        new CapabilityCriteria(
            CapabilityName.CanExplainBug, 3, 3, 2,
            "Able to clearly explain technical issues to non-technical stakeholders"),
        new CapabilityCriteria(
            CapabilityName.CanAskClarification, 2, 2, 4,
            "Able to ask targeted clarifying questions when requirements are unclear"),
        new CapabilityCriteria(
            CapabilityName.CanReportDelay, 2, 2, 3,
            "Able to professionally communicate timeline adjustments to stakeholders"),
        new CapabilityCriteria(
            CapabilityName.CanProposeOptions, 3, 3, 2,
            "Able to present multiple solution options with trade-offs"),
        new CapabilityCriteria(
            CapabilityName.CanSummarizeNextSteps, 3, 2, 3,
            "Able to concisely summarize decisions and action items")
    };

    public static CapabilityCriteria Get(CapabilityName name) =>
        All.First(c => c.Name == name);
}
