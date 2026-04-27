namespace EnglishCoach.Domain.InterviewPractice;

/// <summary>
/// Metadata about the adaptive interviewer's decision for a given turn.
/// Explains why the interviewer chose this specific turn type and capability target.
/// This is internal reasoning — only the LearnerFacingHint (if any) should be shown to the learner.
/// </summary>
public sealed record InterviewTurnDecision
{
    /// <summary>The type of turn the interviewer decided to produce.</summary>
    public InterviewTurnType TurnType { get; init; }

    /// <summary>The capability being assessed by this turn.</summary>
    public InterviewCapability TargetCapability { get; init; }

    /// <summary>Internal reason code explaining the decision (e.g. "shallow_answer", "low_confidence_stt").</summary>
    public string ReasonCode { get; init; } = string.Empty;

    /// <summary>Whether this turn advances the interview plan to the next capability.</summary>
    public bool ShouldAdvancePlan { get; init; }

    /// <summary>Hint shown to learner in TrainingInterview mode only. Null in RealInterview mode.</summary>
    public string? LearnerFacingHint { get; init; }
}
