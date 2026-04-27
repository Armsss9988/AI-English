namespace EnglishCoach.Domain.InterviewPractice;

/// <summary>
/// Evaluation rubric attached to an interviewer question.
/// Tells the evaluator how to assess the learner's answer to this specific question.
/// </summary>
public sealed record InterviewQuestionRubric
{
    /// <summary>Which capability this question targets.</summary>
    public InterviewCapability Capability { get; init; }

    /// <summary>What a successful answer looks like for this question.</summary>
    public string SuccessCriteria { get; init; } = string.Empty;

    /// <summary>What CV evidence is expected in the answer.</summary>
    public string ExpectedCvEvidence { get; init; } = string.Empty;

    /// <summary>What JD signals are relevant for this question.</summary>
    public string JdSignals { get; init; } = string.Empty;

    /// <summary>Suggested answer structure hint (e.g. "Use STAR method").</summary>
    public string AnswerStructureHint { get; init; } = string.Empty;
}
