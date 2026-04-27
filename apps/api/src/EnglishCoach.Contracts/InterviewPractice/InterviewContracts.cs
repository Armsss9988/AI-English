namespace EnglishCoach.Contracts.InterviewPractice;

// ---- CV Upload ----

public record UploadCvRequest(string CvText);

public record UploadCvResponse(Guid ProfileId, string CvAnalysis);

// ---- Start Interview ----

public record StartInterviewRequest(
    Guid ProfileId,
    string JdText,
    string InterviewType // "Mixed", "Behavioral", "Technical", "Situational"
);

public record StartInterviewResponse(
    Guid SessionId,
    string Status,
    string InterviewType,
    int PlannedQuestionCount,
    string FirstQuestion,
    string QuestionCategory,
    string? CoachingHint
);

// ---- Answer Question ----

public record AnswerQuestionRequest(
    string Answer,
    string? AudioUrl
);

public record AnswerQuestionResponse(
    string? NextQuestion,
    string? QuestionCategory,
    string? CoachingHint,
    bool IsInterviewComplete,
    int AnsweredCount,
    int TotalQuestions
);

// ---- Finalize / Feedback ----

public record InterviewFeedbackResponse(
    Guid SessionId,
    int OverallScore,
    int CommunicationScore,
    int TechnicalAccuracyScore,
    int ConfidenceScore,
    string DetailedFeedbackEn,
    string DetailedFeedbackVi,
    List<string> StrengthAreas,
    List<string> ImprovementAreas,
    List<string> SuggestedPhrases,
    string RetryRecommendation
);

// ---- History ----

public record InterviewHistoryResponse(List<InterviewHistoryItem> Sessions);

public record InterviewHistoryItem(
    Guid SessionId,
    string InterviewType,
    string Status,
    int PlannedQuestionCount,
    int AnsweredCount,
    int? OverallScore,
    DateTimeOffset CreatedAt
);
