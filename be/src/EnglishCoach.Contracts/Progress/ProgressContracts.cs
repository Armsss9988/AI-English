namespace EnglishCoach.Contracts.Progress;

public record GetReadinessRequest(Guid LearnerId);

public record GetReadinessResponse(
    Guid LearnerId,
    decimal Score,
    int FormulaVersion,
    IReadOnlyList<ReadinessComponent> Components,
    DateTimeOffset CalculatedAt
);

public record ReadinessComponent(
    string Name,
    decimal Value,
    decimal Weight,
    string Explanation
);

public record GetCapabilitiesRequest(Guid LearnerId);

public record GetCapabilitiesResponse(
    Guid LearnerId,
    IReadOnlyList<CapabilityStatus> Capabilities
);

public record CapabilityStatus(
    string Name,
    string Status,
    string Explanation,
    IReadOnlyList<string> Evidence
);

public enum CapabilityName
{
    CanGiveDailyUpdate,
    CanExplainBug,
    CanAskClarification,
    CanReportDelay,
    CanProposeOptions,
    CanSummarizeNextSteps
}