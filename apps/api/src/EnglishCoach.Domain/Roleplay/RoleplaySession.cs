namespace EnglishCoach.Domain.Roleplay;

public sealed record RoleplaySummary(
    string Result,
    string ClearPoints,
    string TopMistakes,
    string ImprovedAnswer,
    string PhrasesToReview,
    string RetryChallenge
);

public sealed class RoleplaySession
{
    private readonly List<RoleplayTurn> _turns = new();

    private RoleplaySession()
    {
        Id = string.Empty;
        LearnerId = string.Empty;
        ScenarioId = string.Empty;
    }

    public string Id { get; private set; }
    public string LearnerId { get; private set; }
    public string ScenarioId { get; private set; }
    public int ScenarioContentVersion { get; private set; }
    
    public RoleplaySessionState State { get; private set; }
    public IReadOnlyList<RoleplayTurn> Turns => _turns.AsReadOnly();
    
    public RoleplaySummary? Summary { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static RoleplaySession Create(string id, string learnerId, string scenarioId, int scenarioContentVersion)
    {
        return new RoleplaySession
        {
            Id = RequireNonEmpty(id, nameof(id)),
            LearnerId = RequireNonEmpty(learnerId, nameof(learnerId)),
            ScenarioId = RequireNonEmpty(scenarioId, nameof(scenarioId)),
            ScenarioContentVersion = scenarioContentVersion,
            State = RoleplaySessionState.Created,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public void AddClientTurn(string message)
    {
        if (State != RoleplaySessionState.Created && State != RoleplaySessionState.Active)
            throw new InvalidOperationException($"Cannot add client turn in state {State}");

        _turns.Add(RoleplayTurn.Create(Id, TurnRole.Client, message));
        
        if (State == RoleplaySessionState.Created)
        {
            State = RoleplaySessionState.Active;
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void AddLearnerTurn(string message, string audioUrl = "")
    {
        if (State != RoleplaySessionState.Active)
            throw new InvalidOperationException($"Cannot add learner turn in state {State}. Only active sessions accept learner turns.");

        _turns.Add(RoleplayTurn.Create(Id, TurnRole.Learner, message, audioUrl));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RequestFeedback()
    {
        if (State != RoleplaySessionState.Active)
            throw new InvalidOperationException($"Cannot request feedback from state {State}");

        if (!_turns.Any(t => t.Role == TurnRole.Learner))
            throw new InvalidOperationException("Summary cannot exist without at least one learner turn.");

        State = RoleplaySessionState.AwaitingFeedback;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Finalize(RoleplaySummary summary)
    {
        if (State == RoleplaySessionState.Finalized)
            throw new InvalidOperationException("Session cannot be finalized twice.");

        if (State != RoleplaySessionState.AwaitingFeedback)
            throw new InvalidOperationException($"Cannot finalize from state {State}. Must request feedback first.");

        if (!_turns.Any(t => t.Role == TurnRole.Learner))
            throw new InvalidOperationException("Finalization requires at least one learner turn.");

        Summary = summary ?? throw new ArgumentNullException(nameof(summary));
        State = RoleplaySessionState.Finalized;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        if (State == RoleplaySessionState.Archived)
            return;

        State = RoleplaySessionState.Archived;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string RequireNonEmpty(string value, string paramName) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", paramName) : value.Trim();
}
