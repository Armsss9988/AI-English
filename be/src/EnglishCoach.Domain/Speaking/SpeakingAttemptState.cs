namespace EnglishCoach.Domain.Speaking;

public enum SpeakingAttemptState
{
    Created,
    Uploaded,
    Transcribed,
    Evaluating,
    Evaluated,
    EvaluationFailed
}

public static class SpeakingAttemptStateTransitions
{
    private static readonly Dictionary<SpeakingAttemptState, HashSet<SpeakingAttemptState>> _allowedTransitions = new()
    {
        [SpeakingAttemptState.Created] = new() { SpeakingAttemptState.Uploaded },
        [SpeakingAttemptState.Uploaded] = new() { SpeakingAttemptState.Transcribed },
        [SpeakingAttemptState.Transcribed] = new() { SpeakingAttemptState.Evaluating },
        [SpeakingAttemptState.Evaluating] = new() { SpeakingAttemptState.Evaluated, SpeakingAttemptState.EvaluationFailed },
        [SpeakingAttemptState.Evaluated] = new(),
        [SpeakingAttemptState.EvaluationFailed] = new() { SpeakingAttemptState.Evaluating }, // Can retry
    };

    public static bool CanTransition(SpeakingAttemptState from, SpeakingAttemptState to) =>
        _allowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}