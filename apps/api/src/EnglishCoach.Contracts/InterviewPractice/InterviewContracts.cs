namespace EnglishCoach.Contracts.InterviewPractice;

// ── T01: Domain concept contracts ──

public record InterviewCapabilityDto(string Value, string Label);

public record InterviewTurnTypeDto(string Value, string Label);

// ── CV Upload ──

public record UploadCvRequest(string CvText);

public record UploadCvResponse(Guid ProfileId, string CvAnalysis);

// ── Start Interview ──

public record StartInterviewRequest(
    Guid ProfileId,
    string JdText,
    string InterviewType, // "Mixed", "Behavioral", "Technical", "Situational"
    string InterviewMode = "TrainingInterview" // "RealInterview" or "TrainingInterview"
);

public record StartInterviewResponse(
    Guid SessionId,
    string Status,
    string InterviewType,
    string InterviewMode,
    int PlannedQuestionCount,
    string FirstQuestion,
    string QuestionCategory,
    string? TurnType,
    string? TargetCapability,
    string? CoachingHint,
    string? AudioUrl
);

// ── Answer Question ──

public record AnswerQuestionRequest(
    string Answer,
    string? AudioUrl
);

public record AnswerQuestionResponse(
    string? NextQuestion,
    string? QuestionCategory,
    string? TurnType,
    string? TargetCapability,
    string? CoachingHint,
    bool IsInterviewComplete,
    int AnsweredCount,
    int TotalQuestions,
    string? AudioUrl
);

// ── Transcript Confirmation (T06) ──

public record ConfirmTranscriptRequest(
    string ConfirmedTranscript,
    bool LearnerEdited
);

public record TranscriptResponse(
    string TurnId,
    string RawTranscript,
    double Confidence,
    string TurnState
);

// ── Finalize / Feedback ──

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

// ── History ──

public record InterviewHistoryResponse(List<InterviewHistoryItem> Sessions);

public record InterviewHistoryItem(
    Guid SessionId,
    string InterviewType,
    string InterviewMode,
    string Status,
    int PlannedQuestionCount,
    int AnsweredCount,
    int? OverallScore,
    DateTimeOffset CreatedAt
);

// ── Session Detail (T10) ──

public record InterviewSessionDetailResponse(
    Guid SessionId,
    string InterviewType,
    string InterviewMode,
    string Status,
    int PlannedQuestionCount,
    int AnsweredCount,
    string JdSummary,
    List<InterviewTurnDto> Turns,
    InterviewFeedbackResponse? Feedback
);

public record InterviewTurnDto(
    string TurnId,
    string Role,
    string Message,
    string? TurnType,
    string? TargetCapability,
    string? Category,
    string? AudioUrl,
    int? AudioDurationMs,
    string? RawTranscript,
    string? ConfirmedTranscript,
    double? TranscriptConfidence,
    string? CoachingHint,
    string TurnState,
    string VerificationStatus,
    DateTimeOffset CreatedAt
);
