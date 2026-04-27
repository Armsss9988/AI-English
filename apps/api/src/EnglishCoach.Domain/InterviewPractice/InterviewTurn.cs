namespace EnglishCoach.Domain.InterviewPractice;

public enum InterviewTurnRole
{
    Interviewer,
    Learner
}

public sealed class InterviewTurn
{
    private InterviewTurn()
    {
        Id = string.Empty;
        SessionId = string.Empty;
        Message = string.Empty;
        AudioUrl = string.Empty;
    }

    public string Id { get; private set; }
    public string SessionId { get; private set; }
    public InterviewTurnRole Role { get; private set; }
    public string Message { get; private set; }
    public string AudioUrl { get; private set; }
    public int TurnOrder { get; private set; }
    public InterviewQuestionCategory? QuestionCategory { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static InterviewTurn Create(
        string sessionId,
        InterviewTurnRole role,
        string message,
        int turnOrder,
        InterviewQuestionCategory? questionCategory = null,
        string audioUrl = "")
    {
        return new InterviewTurn
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = RequireNonEmpty(sessionId, nameof(sessionId)),
            Role = role,
            Message = message,
            AudioUrl = audioUrl ?? string.Empty,
            TurnOrder = turnOrder,
            QuestionCategory = questionCategory,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private static string RequireNonEmpty(string value, string paramName) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", paramName) : value.Trim();
}
