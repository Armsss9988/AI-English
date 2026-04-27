namespace EnglishCoach.Domain.InterviewPractice;

/// <summary>
/// Core aggregate for a mock interview session. 
/// Contains JD analysis, turn-by-turn conversation, and final feedback.
/// State machine: Created → Analyzing → Ready → Active → AwaitingFeedback → Finalized → Archived
/// InterviewMode: RealInterview (no hints) or TrainingInterview (hints, retry, scorecard).
/// </summary>
public sealed class InterviewSession
{
    private readonly List<InterviewTurn> _turns = new();

    private InterviewSession()
    {
        Id = string.Empty;
        LearnerId = string.Empty;
        InterviewProfileId = string.Empty;
        JdText = string.Empty;
        JdAnalysis = string.Empty;
        InterviewPlan = string.Empty;
    }

    public string Id { get; private set; }
    public string LearnerId { get; private set; }
    public string InterviewProfileId { get; private set; }
    public string JdText { get; private set; }

    /// <summary>
    /// JSON-structured analysis of the JD (requirements, company context, key skills).
    /// </summary>
    public string JdAnalysis { get; private set; }

    /// <summary>
    /// JSON-structured interview plan: question count, question outline, focus areas.
    /// </summary>
    public string InterviewPlan { get; private set; }

    public InterviewType Type { get; private set; }
    public InterviewMode Mode { get; private set; }
    public InterviewSessionState State { get; private set; }

    /// <summary>
    /// Dynamic question count determined by CV/JD complexity analysis.
    /// </summary>
    public int PlannedQuestionCount { get; private set; }

    public IReadOnlyList<InterviewTurn> Turns => _turns.AsReadOnly();
    public InterviewFeedback? Feedback { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static InterviewSession Create(
        string id,
        string learnerId,
        string interviewProfileId,
        string jdText,
        InterviewType type,
        InterviewMode mode = InterviewMode.TrainingInterview)
    {
        return new InterviewSession
        {
            Id = RequireNonEmpty(id, nameof(id)),
            LearnerId = RequireNonEmpty(learnerId, nameof(learnerId)),
            InterviewProfileId = RequireNonEmpty(interviewProfileId, nameof(interviewProfileId)),
            JdText = RequireNonEmpty(jdText, nameof(jdText)),
            Type = type,
            Mode = mode,
            State = InterviewSessionState.Created,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public void StartAnalysis()
    {
        if (State != InterviewSessionState.Created)
            throw new InvalidOperationException($"Cannot start analysis from state {State}.");

        State = InterviewSessionState.Analyzing;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void CompleteAnalysis(string jdAnalysis, string interviewPlan, int plannedQuestionCount)
    {
        if (State != InterviewSessionState.Analyzing)
            throw new InvalidOperationException($"Cannot complete analysis from state {State}.");

        JdAnalysis = jdAnalysis ?? throw new ArgumentNullException(nameof(jdAnalysis));
        InterviewPlan = interviewPlan ?? throw new ArgumentNullException(nameof(interviewPlan));
        PlannedQuestionCount = plannedQuestionCount > 0
            ? plannedQuestionCount
            : throw new ArgumentException("Planned question count must be positive.", nameof(plannedQuestionCount));
        State = InterviewSessionState.Ready;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void AddInterviewerTurn(string message, InterviewQuestionCategory category)
    {
        if (State != InterviewSessionState.Ready && State != InterviewSessionState.Active)
            throw new InvalidOperationException($"Cannot add interviewer turn in state {State}.");

        var turnOrder = _turns.Count + 1;
        _turns.Add(InterviewTurn.Create(Id, InterviewTurnRole.Interviewer, message, turnOrder, category));

        if (State == InterviewSessionState.Ready)
        {
            State = InterviewSessionState.Active;
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Add an adaptive interviewer turn with rich metadata.
    /// </summary>
    public InterviewTurn AddAdaptiveInterviewerTurn(
        string message,
        InterviewTurnType turnType,
        InterviewCapability targetCapability,
        InterviewQuestionRubric? rubric,
        InterviewTurnDecision? decision,
        InterviewVerificationStatus verificationStatus)
    {
        if (State != InterviewSessionState.Ready && State != InterviewSessionState.Active)
            throw new InvalidOperationException($"Cannot add interviewer turn in state {State}.");

        var turnOrder = _turns.Count + 1;
        var turn = InterviewTurn.CreateInterviewerTurn(
            Id, message, turnOrder, turnType, targetCapability,
            rubric, decision, verificationStatus);
        _turns.Add(turn);

        if (State == InterviewSessionState.Ready)
        {
            State = InterviewSessionState.Active;
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return turn;
    }

    public void AddLearnerTurn(string message, string audioUrl = "")
    {
        if (State != InterviewSessionState.Active)
            throw new InvalidOperationException($"Cannot add learner turn in state {State}. Only active sessions accept learner turns.");

        var turnOrder = _turns.Count + 1;
        _turns.Add(InterviewTurn.Create(Id, InterviewTurnRole.Learner, message, turnOrder, audioUrl: audioUrl));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RequestFeedback()
    {
        if (State != InterviewSessionState.Active)
            throw new InvalidOperationException($"Cannot request feedback from state {State}.");

        if (!_turns.Any(t => t.Role == InterviewTurnRole.Learner))
            throw new InvalidOperationException("Cannot finalize without at least one learner answer.");

        State = InterviewSessionState.AwaitingFeedback;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetFeedback(InterviewFeedback feedback)
    {
        if (State == InterviewSessionState.Finalized)
            throw new InvalidOperationException("Session cannot be finalized twice.");

        if (State != InterviewSessionState.AwaitingFeedback)
            throw new InvalidOperationException($"Cannot finalize from state {State}. Must request feedback first.");

        Feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
        State = InterviewSessionState.Finalized;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        if (State == InterviewSessionState.Archived)
            return;

        State = InterviewSessionState.Archived;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// How many learner answers have been recorded so far.
    /// </summary>
    public int LearnerAnswerCount => _turns.Count(t => t.Role == InterviewTurnRole.Learner);

    /// <summary>
    /// Whether the interview has reached its planned question limit.
    /// </summary>
    public bool IsQuestionLimitReached => PlannedQuestionCount > 0 && LearnerAnswerCount >= PlannedQuestionCount;

    private static string RequireNonEmpty(string value, string paramName) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", paramName) : value.Trim();
}
