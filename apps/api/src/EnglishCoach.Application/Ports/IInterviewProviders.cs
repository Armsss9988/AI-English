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

// ---- T02: Adaptive Interviewer Provider ----

public interface IAdaptiveInterviewerService
{
    Task<InterviewTurnGenerationResult> GenerateInterviewerTurnAsync(
        InterviewTurnGenerationContext context, CancellationToken ct = default);
    Task<AnswerEvaluationResult> EvaluateAnswerAsync(
        AnswerEvaluationContext context, CancellationToken ct = default);
    ProviderKind Provider { get; }
}

// ---- T04: Text-to-Speech ----

public interface ITextToSpeechService
{
    Task<TextToSpeechResult> SynthesizeAsync(TextToSpeechRequest request, CancellationToken ct = default);
    ProviderKind Provider { get; }
}

// ---- T04/T05: Audio Storage ----

public interface IInterviewAudioStorage
{
    Task<AudioStorageResult> SaveAsync(AudioStorageRequest request, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default);
    Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default);
}

// ---- T06: Speech-to-Text ----

public interface ISpeechToTextService
{
    Task<SpeechToTextResult> TranscribeAsync(SpeechToTextRequest request, CancellationToken ct = default);
    ProviderKind Provider { get; }
}

// ---- T07: Pronunciation Assessment ----

public interface IPronunciationAssessmentService
{
    Task<PronunciationAssessmentResult> AssessAsync(
        PronunciationAssessmentRequest request, CancellationToken ct = default);
    ProviderKind Provider { get; }
}

// ==================== Result Records ====================

public record CvAnalysisResult
{
    public bool IsSuccess { get; init; }
    public string? Analysis { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static CvAnalysisResult Success(string analysis, ProviderKind provider) => new()
    { IsSuccess = true, Analysis = analysis, Provider = provider };
    public static CvAnalysisResult Failure(string errorMessage, ProviderKind provider) => new()
    { IsSuccess = false, ErrorMessage = errorMessage, Provider = provider };
}

public record JdAnalysisResult
{
    public bool IsSuccess { get; init; }
    public string? Analysis { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static JdAnalysisResult Success(string analysis, ProviderKind provider) => new()
    { IsSuccess = true, Analysis = analysis, Provider = provider };
    public static JdAnalysisResult Failure(string errorMessage, ProviderKind provider) => new()
    { IsSuccess = false, ErrorMessage = errorMessage, Provider = provider };
}

public record InterviewPlanResult
{
    public bool IsSuccess { get; init; }
    public string? Plan { get; init; }
    public int RecommendedQuestionCount { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static InterviewPlanResult Success(string plan, int questionCount, ProviderKind provider) => new()
    { IsSuccess = true, Plan = plan, RecommendedQuestionCount = questionCount, Provider = provider };
    public static InterviewPlanResult Failure(string errorMessage, ProviderKind provider) => new()
    { IsSuccess = false, ErrorMessage = errorMessage, Provider = provider };
}

// ---- Legacy Conductor Context (kept for backward compat) ----

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

// ---- T02: Adaptive Turn Generation ----

public record InterviewTurnGenerationContext
{
    public string SessionId { get; init; } = string.Empty;
    public InterviewMode InterviewMode { get; init; }
    public string CvAnalysis { get; init; } = string.Empty;
    public string JdAnalysis { get; init; } = string.Empty;
    public string InterviewPlan { get; init; } = string.Empty;
    public InterviewCapability? CurrentCapabilityTarget { get; init; }
    public int CurrentQuestionNumber { get; init; }
    public int PlannedQuestionCount { get; init; }
    public IReadOnlyList<InterviewTurnRecord> ConversationHistory { get; init; } = Array.Empty<InterviewTurnRecord>();
    public string? LatestLearnerTranscript { get; init; }
    public string? LatestScorecardJson { get; init; }
    public string? LatestPronunciationReportJson { get; init; }
    public InterviewTurnDecision? PreviousTurnDecision { get; init; }
}

public record InterviewTurnGenerationResult
{
    public bool IsSuccess { get; init; }
    public string Question { get; init; } = string.Empty;
    public InterviewTurnType TurnType { get; init; }
    public InterviewCapability TargetCapability { get; init; }
    public InterviewQuestionRubric? Rubric { get; init; }
    public bool ShouldAdvancePlan { get; init; }
    public string ReasonCode { get; init; } = string.Empty;
    public string? LearnerFacingHint { get; init; }
    public bool IsLastQuestion { get; init; }
    public bool UsedFallback { get; init; }
    public ProviderKind Provider { get; init; }
    public string ModelId { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }

    public static InterviewTurnGenerationResult Success(
        string question, InterviewTurnType turnType, InterviewCapability capability,
        InterviewQuestionRubric? rubric, bool shouldAdvance, string reasonCode,
        string? hint, bool isLast, ProviderKind provider, string modelId, bool usedFallback = false) => new()
    {
        IsSuccess = true, Question = question, TurnType = turnType,
        TargetCapability = capability, Rubric = rubric, ShouldAdvancePlan = shouldAdvance,
        ReasonCode = reasonCode, LearnerFacingHint = hint, IsLastQuestion = isLast,
        Provider = provider, ModelId = modelId, UsedFallback = usedFallback
    };

    public static InterviewTurnGenerationResult Failure(string error, ProviderKind provider) => new()
    { IsSuccess = false, ErrorMessage = error, Provider = provider };
}

// ---- T08: Answer Evaluation ----

public record AnswerEvaluationContext
{
    public string SessionId { get; init; } = string.Empty;
    public InterviewMode InterviewMode { get; init; }
    public string CvAnalysis { get; init; } = string.Empty;
    public string JdAnalysis { get; init; } = string.Empty;
    public string QuestionText { get; init; } = string.Empty;
    public string ConfirmedTranscript { get; init; } = string.Empty;
    public InterviewQuestionRubric? Rubric { get; init; }
    public string? PronunciationReportJson { get; init; }
    public InterviewCapability TargetCapability { get; init; }
}

public record AnswerEvaluationResult
{
    public bool IsSuccess { get; init; }
    public AnswerScorecardContent? Scorecard { get; init; }
    public string? ErrorMessage { get; init; }
    public bool UsedFallback { get; init; }
    public ProviderKind Provider { get; init; }

    public static AnswerEvaluationResult Success(AnswerScorecardContent card, ProviderKind provider, bool fallback = false) => new()
    { IsSuccess = true, Scorecard = card, Provider = provider, UsedFallback = fallback };
    public static AnswerEvaluationResult Failure(string error, ProviderKind provider) => new()
    { IsSuccess = false, ErrorMessage = error, Provider = provider };
}

public record AnswerScorecardContent
{
    public int ContentFitScore { get; init; }
    public int JdRelevanceScore { get; init; }
    public int CvEvidenceScore { get; init; }
    public int StructureScore { get; init; }
    public int TechnicalCredibilityScore { get; init; }
    public int EnglishClarityScore { get; init; }
    public int ProfessionalToneScore { get; init; }
    public int PronunciationClarityScore { get; init; }
    public int FluencyScore { get; init; }
    public int OverallScore { get; init; }
    public string Evidence { get; init; } = string.Empty;
    public string MissingEvidence { get; init; } = string.Empty;
    public string BetterAnswer { get; init; } = string.Empty;
    public IReadOnlyList<AnswerCorrection> Corrections { get; init; } = Array.Empty<AnswerCorrection>();
    public string RetryDrillPrompt { get; init; } = string.Empty;
    public IReadOnlyList<string> PhraseCandidates { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MistakeCandidates { get; init; } = Array.Empty<string>();
    public bool RequiresRetry { get; init; }
}

public record AnswerCorrection
{
    public string Original { get; init; } = string.Empty;
    public string Corrected { get; init; } = string.Empty;
    public string ExplanationVi { get; init; } = string.Empty;
}

// ---- Legacy result records ----

public record InterviewQuestionResult
{
    public bool IsSuccess { get; init; }
    public InterviewQuestionContent? Content { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static InterviewQuestionResult Success(InterviewQuestionContent content, ProviderKind provider) => new()
    { IsSuccess = true, Content = content, Provider = provider };
    public static InterviewQuestionResult Failure(string errorMessage, ProviderKind provider) => new()
    { IsSuccess = false, ErrorMessage = errorMessage, Provider = provider };
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
    { IsSuccess = true, Content = content, Provider = provider };
    public static InterviewFeedbackResult Failure(string errorMessage, ProviderKind provider) => new()
    { IsSuccess = false, ErrorMessage = errorMessage, Provider = provider };
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

// ---- T04: TTS Records ----

public record TextToSpeechRequest
{
    public string Text { get; init; } = string.Empty;
    public string VoiceId { get; init; } = "en-US-default";
    public double SpeakingRate { get; init; } = 1.0;
    public string Purpose { get; init; } = "InterviewerTurn"; // InterviewerTurn or PronunciationExample
}

public record TextToSpeechResult
{
    public bool IsSuccess { get; init; }
    public byte[]? AudioData { get; init; }
    public string ContentType { get; init; } = "audio/mp3";
    public int DurationMs { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static TextToSpeechResult Success(byte[] data, string contentType, int durationMs, ProviderKind provider) => new()
    { IsSuccess = true, AudioData = data, ContentType = contentType, DurationMs = durationMs, Provider = provider };
    public static TextToSpeechResult Failure(string error, ProviderKind provider) => new()
    { IsSuccess = false, ErrorMessage = error, Provider = provider };
}

// ---- T04/T05: Audio Storage Records ----

public record AudioStorageRequest
{
    public string SessionId { get; init; } = string.Empty;
    public string TurnId { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty; // "interviewer" or "learner"
    public byte[] AudioData { get; init; } = Array.Empty<byte>();
    public string ContentType { get; init; } = "audio/webm";
}

public record AudioStorageResult
{
    public bool IsSuccess { get; init; }
    public string StorageKey { get; init; } = string.Empty;
    public long ByteSize { get; init; }
    public string? ErrorMessage { get; init; }

    public static AudioStorageResult Success(string key, long size) => new()
    { IsSuccess = true, StorageKey = key, ByteSize = size };
    public static AudioStorageResult Failure(string error) => new()
    { IsSuccess = false, ErrorMessage = error };
}

// ---- T06: STT Records ----

public record SpeechToTextRequest
{
    public string AudioStorageKey { get; init; } = string.Empty;
    public string ContentType { get; init; } = "audio/webm";
    public string? LanguageHint { get; init; } = "en-US";
}

public record SpeechToTextResult
{
    public bool IsSuccess { get; init; }
    public string Transcript { get; init; } = string.Empty;
    public string Language { get; init; } = "en";
    public double Confidence { get; init; }
    public IReadOnlyList<WordTiming>? WordTimings { get; init; }
    public bool UsedFallback { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static SpeechToTextResult Success(string transcript, double confidence, ProviderKind provider,
        IReadOnlyList<WordTiming>? wordTimings = null, bool usedFallback = false) => new()
    {
        IsSuccess = true, Transcript = transcript, Confidence = confidence,
        Provider = provider, WordTimings = wordTimings, UsedFallback = usedFallback
    };
    public static SpeechToTextResult Failure(string error, ProviderKind provider) => new()
    { IsSuccess = false, ErrorMessage = error, Provider = provider };
}

public record WordTiming
{
    public string Word { get; init; } = string.Empty;
    public double Confidence { get; init; }
    public int StartMs { get; init; }
    public int EndMs { get; init; }
}

// ---- T07: Pronunciation Records ----

public record PronunciationAssessmentRequest
{
    public string AudioStorageKey { get; init; } = string.Empty;
    public string RawTranscript { get; init; } = string.Empty;
    public string ConfirmedTranscript { get; init; } = string.Empty;
    public IReadOnlyList<WordTiming>? WordConfidences { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public IReadOnlyList<string> ImportantTerms { get; init; } = Array.Empty<string>();
}

public record PronunciationAssessmentResult
{
    public bool IsSuccess { get; init; }
    public int OverallScore { get; init; }
    public int FluencyScore { get; init; }
    public int AccuracyScore { get; init; }
    public int CompletenessScore { get; init; }
    public IReadOnlyList<PronunciationWordIssue> WordIssues { get; init; } = Array.Empty<PronunciationWordIssue>();
    public bool UsedFallback { get; init; }
    public string? ErrorMessage { get; init; }
    public ProviderKind Provider { get; init; }

    public static PronunciationAssessmentResult Success(
        int overall, int fluency, int accuracy, int completeness,
        IReadOnlyList<PronunciationWordIssue> issues, ProviderKind provider, bool fallback = false) => new()
    {
        IsSuccess = true, OverallScore = overall, FluencyScore = fluency,
        AccuracyScore = accuracy, CompletenessScore = completeness,
        WordIssues = issues, Provider = provider, UsedFallback = fallback
    };
    public static PronunciationAssessmentResult Failure(string error, ProviderKind provider) => new()
    { IsSuccess = false, ErrorMessage = error, Provider = provider };
}

public record PronunciationWordIssue
{
    public string HeardAs { get; init; } = string.Empty;
    public string Expected { get; init; } = string.Empty;
    public string IssueType { get; init; } = string.Empty; // EndingSound, WordStress, Vowel, etc.
    public string ExplanationVi { get; init; } = string.Empty;
    public string CorrectPronunciationText { get; init; } = string.Empty;
    public string? Ipa { get; init; }
    public string? ExampleAudioUrl { get; init; }
    public string Severity { get; init; } = "Medium"; // Low, Medium, High
}
