namespace EnglishCoach.Domain.InterviewPractice;

/// <summary>
/// Tracks whether an interview output was produced by a real provider or a fallback.
/// Only Verified outputs can improve readiness and progress.
/// </summary>
public enum InterviewVerificationStatus
{
    /// <summary>Output was produced by a real, configured AI provider.</summary>
    Verified,

    /// <summary>Output was produced by a fallback or degraded path.</summary>
    Unverified,

    /// <summary>Output was produced by a fake/stub provider (local dev/tests only).</summary>
    Fallback
}
