namespace EnglishCoach.Domain.InterviewPractice;

/// <summary>
/// Describes the nature of an interviewer turn.
/// Used by the adaptive interviewer to communicate why this turn was chosen.
/// </summary>
public enum InterviewTurnType
{
    OpeningQuestion,
    MainQuestion,
    FollowUp,
    Clarification,
    Challenge,
    Transition,
    Closing
}

public static class InterviewTurnTypeExtensions
{
    public static InterviewTurnType ParseTurnType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Turn type value is required.", nameof(value));

        if (Enum.TryParse<InterviewTurnType>(value, ignoreCase: true, out var result))
            return result;

        throw new ArgumentException(
            $"Unknown interview turn type: '{value}'. Valid values: {string.Join(", ", Enum.GetNames<InterviewTurnType>())}",
            nameof(value));
    }

    public static bool TryParseTurnType(string value, out InterviewTurnType result)
    {
        return Enum.TryParse(value, ignoreCase: true, out result)
               && Enum.IsDefined(result);
    }
}
