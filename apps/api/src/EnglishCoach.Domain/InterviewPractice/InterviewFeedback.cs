namespace EnglishCoach.Domain.InterviewPractice;

/// <summary>
/// Value object: comprehensive feedback after a mock interview session.
/// Bilingual support (English + Vietnamese).
/// </summary>
public sealed record InterviewFeedback(
    int OverallScore,
    int CommunicationScore,
    int TechnicalAccuracyScore,
    int ConfidenceScore,
    string DetailedFeedbackEn,
    string DetailedFeedbackVi,
    IReadOnlyList<string> StrengthAreas,
    IReadOnlyList<string> ImprovementAreas,
    IReadOnlyList<string> SuggestedPhrases,
    string RetryRecommendation
);
