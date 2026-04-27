namespace EnglishCoach.Domain.InterviewPractice;

/// <summary>
/// Stores parsed CV information for a learner. 
/// Can be reused across multiple interview sessions.
/// </summary>
public sealed class InterviewProfile
{
    private InterviewProfile()
    {
        Id = string.Empty;
        LearnerId = string.Empty;
        CvText = string.Empty;
        CvAnalysis = string.Empty;
    }

    public string Id { get; private set; }
    public string LearnerId { get; private set; }
    public string CvText { get; private set; }

    /// <summary>
    /// JSON-structured analysis of the CV (skills, experience, strengths, weaknesses).
    /// Populated by AI analysis service.
    /// </summary>
    public string CvAnalysis { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static InterviewProfile Create(string id, string learnerId, string cvText)
    {
        return new InterviewProfile
        {
            Id = RequireNonEmpty(id, nameof(id)),
            LearnerId = RequireNonEmpty(learnerId, nameof(learnerId)),
            CvText = RequireNonEmpty(cvText, nameof(cvText)),
            CvAnalysis = string.Empty,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public void SetCvAnalysis(string analysis)
    {
        CvAnalysis = RemoveUnsupportedStorageCharacters(analysis ?? throw new ArgumentNullException(nameof(analysis)));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateCv(string newCvText)
    {
        CvText = RequireNonEmpty(newCvText, nameof(newCvText));
        CvAnalysis = string.Empty; // Reset analysis when CV changes
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string RequireNonEmpty(string value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName);

        var normalized = RemoveUnsupportedStorageCharacters(value).Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? throw new ArgumentException("Value is required.", paramName)
            : normalized;
    }

    private static string RemoveUnsupportedStorageCharacters(string value)
    {
        return value.Replace("\0", string.Empty, StringComparison.Ordinal);
    }
}
