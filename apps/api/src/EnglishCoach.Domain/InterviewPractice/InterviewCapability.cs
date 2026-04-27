namespace EnglishCoach.Domain.InterviewPractice;

/// <summary>
/// Capabilities assessed during an interview session.
/// Each capability represents a distinct area of interview performance.
/// </summary>
public enum InterviewCapability
{
    SelfIntroduction,
    ProjectDeepDive,
    TechnicalTradeoff,
    BehavioralStar,
    ClientCommunication,
    RequirementClarification,
    IncidentConflictStory,
    WeakSpotRetry,
    EnglishClarity,
    PronunciationClarity
}

public static class InterviewCapabilityExtensions
{
    private static readonly HashSet<string> ValidValues = new(
        Enum.GetNames<InterviewCapability>(),
        StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parse a string to InterviewCapability. Throws if unknown.
    /// </summary>
    public static InterviewCapability ParseCapability(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Capability value is required.", nameof(value));

        if (Enum.TryParse<InterviewCapability>(value, ignoreCase: true, out var result))
            return result;

        throw new ArgumentException(
            $"Unknown interview capability: '{value}'. Valid values: {string.Join(", ", ValidValues)}",
            nameof(value));
    }

    /// <summary>
    /// Try parse without throwing.
    /// </summary>
    public static bool TryParseCapability(string value, out InterviewCapability result)
    {
        return Enum.TryParse(value, ignoreCase: true, out result)
               && Enum.IsDefined(result);
    }
}
