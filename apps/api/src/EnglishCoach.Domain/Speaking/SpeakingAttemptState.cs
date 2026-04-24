namespace EnglishCoach.Domain.Speaking;

public enum SpeakingAttemptState
{
    Created,
    Uploaded,
    Transcribed,
    Evaluated,
    Finalized
}

public static class SpeakingAttemptStateTransitions
{
    private static readonly Dictionary<SpeakingAttemptState, HashSet<SpeakingAttemptState>> _allowedTransitions = new()
    {
        [SpeakingAttemptState.Created] = new() { SpeakingAttemptState.Uploaded, SpeakingAttemptState.Transcribed }, // Transcribed directly for MVP
        [SpeakingAttemptState.Uploaded] = new() { SpeakingAttemptState.Transcribed },
        [SpeakingAttemptState.Transcribed] = new() { SpeakingAttemptState.Evaluated },
        [SpeakingAttemptState.Evaluated] = new() { SpeakingAttemptState.Finalized },
        [SpeakingAttemptState.Finalized] = new()
    };

    public static bool CanTransition(SpeakingAttemptState from, SpeakingAttemptState to) =>
        _allowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}
