using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Application.Ports;

// ==================== Interview Practice ====================

public interface IInterviewAnalysisService
{
    Task<CvAnalysisResult> AnalyzeCvAsync(string cvText, CancellationToken ct = default);
    Task<JdAnalysisResult> AnalyzeJdAsync(string jdText, string cvAnalysis, CancellationToken ct = default);
    Task<InterviewPlanResult> CreateInterviewPlanAsync(
        string cvAnalysis, string jdAnalysis, InterviewType interviewType, CancellationToken ct = default);
    ProviderKind Provider { get; }
}

public interface IInterviewConductorService
{
    Task<InterviewQuestionResult> GenerateNextQuestionAsync(
        InterviewConductorContext context, CancellationToken ct = default);
    Task<InterviewFeedbackResult> EvaluateSessionAsync(
        InterviewConductorContext context, CancellationToken ct = default);
    ProviderKind Provider { get; }
}

// ---- Result Records ----

public record CvAnalysisResult
{
    public bool IsSuccess { get; init; }
    public string? Analysis { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static CvAnalysisResult Success(string analysis, ProviderKind provider) => new()
    {
        IsSuccess = true,
        Analysis = analysis,
        Provider = provider
    };

    public static CvAnalysisResult Failure(string errorMessage, ProviderKind provider) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Provider = provider
    };
}

public record JdAnalysisResult
{
    public bool IsSuccess { get; init; }
    public string? Analysis { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static JdAnalysisResult Success(string analysis, ProviderKind provider) => new()
    {
        IsSuccess = true,
        Analysis = analysis,
        Provider = provider
    };

    public static JdAnalysisResult Failure(string errorMessage, ProviderKind provider) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Provider = provider
    };
}

public record InterviewPlanResult
{
    public bool IsSuccess { get; init; }
    public string? Plan { get; init; }
    public int RecommendedQuestionCount { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static InterviewPlanResult Success(string plan, int questionCount, ProviderKind provider) => new()
    {
        IsSuccess = true,
        Plan = plan,
        RecommendedQuestionCount = questionCount,
        Provider = provider
    };

    public static InterviewPlanResult Failure(string errorMessage, ProviderKind provider) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Provider = provider
    };
}

public record InterviewConductorContext
{
    public string SessionId { get; init; } = string.Empty;
    public string CvAnalysis { get; init; } = string.Empty;
    public string JdAnalysis { get; init; } = string.Empty;
    public string InterviewPlan { get; init; } = string.Empty;
    public InterviewType InterviewType { get; init; }
    public IReadOnlyList<InterviewTurnRecord> ConversationHistory { get; init; } = Array.Empty<InterviewTurnRecord>();
    public InterviewTurnRecord? LatestLearnerAnswer { get; init; }
    public int PlannedQuestionCount { get; init; }
    public int CurrentQuestionNumber { get; init; }
}

public record InterviewTurnRecord
{
    public string Speaker { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? QuestionCategory { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public record InterviewQuestionResult
{
    public bool IsSuccess { get; init; }
    public InterviewQuestionContent? Content { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static InterviewQuestionResult Success(InterviewQuestionContent content, ProviderKind provider) => new()
    {
        IsSuccess = true,
        Content = content,
        Provider = provider
    };

    public static InterviewQuestionResult Failure(string errorMessage, ProviderKind provider) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Provider = provider
    };
}

public record InterviewQuestionContent
{
    public string Question { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? CoachingHint { get; init; }
    public bool IsLastQuestion { get; init; }
}

public record InterviewFeedbackResult
{
    public bool IsSuccess { get; init; }
    public InterviewFeedbackContent? Content { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static InterviewFeedbackResult Success(InterviewFeedbackContent content, ProviderKind provider) => new()
    {
        IsSuccess = true,
        Content = content,
        Provider = provider
    };

    public static InterviewFeedbackResult Failure(string errorMessage, ProviderKind provider) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Provider = provider
    };
}

public record InterviewFeedbackContent
{
    public int OverallScore { get; init; }
    public int CommunicationScore { get; init; }
    public int TechnicalAccuracyScore { get; init; }
    public int ConfidenceScore { get; init; }
    public string DetailedFeedbackEn { get; init; } = string.Empty;
    public string DetailedFeedbackVi { get; init; } = string.Empty;
    public IReadOnlyList<string> StrengthAreas { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ImprovementAreas { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SuggestedPhrases { get; init; } = Array.Empty<string>();
    public string RetryRecommendation { get; init; } = string.Empty;
}
