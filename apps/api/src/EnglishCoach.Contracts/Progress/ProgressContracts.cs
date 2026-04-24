namespace EnglishCoach.Contracts.Progress;

public record ReadinessResponse(
    decimal OverallScore,
    int FormulaVersion,
    string Trend,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<ReadinessComponentResponse> Components
);

public record ReadinessComponentResponse(
    string Name,
    decimal RawValue,
    decimal Weight,
    decimal WeightedValue,
    string Explanation
);

public record CapabilityMatrixResponse(
    IReadOnlyList<CapabilityResponse> Capabilities
);

public record CapabilityResponse(
    string Name,
    string Status,
    string Explanation,
    IReadOnlyList<string> Evidence
);
