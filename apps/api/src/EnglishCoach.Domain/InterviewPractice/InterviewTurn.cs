using System.Text.Json;

namespace EnglishCoach.Domain.InterviewPractice;

public enum InterviewTurnRole
{
    Interviewer,
    Learner
}

public sealed class InterviewTurn
{
    private InterviewTurn()
    {
        Id = string.Empty;
        SessionId = string.Empty;
        Message = string.Empty;
        AudioUrl = string.Empty;
        AudioStorageKey = string.Empty;
        RawTranscript = string.Empty;
        ConfirmedTranscript = string.Empty;
        RubricJson = string.Empty;
        DecisionJson = string.Empty;
        PronunciationReportJson = string.Empty;
        ScorecardJson = string.Empty;
    }

    public string Id { get; private set; }
    public string SessionId { get; private set; }
    public InterviewTurnRole Role { get; private set; }
    public string Message { get; private set; }

    // ── Legacy fields (kept for backward compatibility) ──
    public string AudioUrl { get; private set; }
    public int TurnOrder { get; private set; }
    public InterviewQuestionCategory? QuestionCategory { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    // ── T01: Adaptive interview domain fields ──

    /// <summary>Semantic turn type (FollowUp, Challenge, Clarification, etc.).</summary>
    public InterviewTurnType? TurnType { get; private set; }

    /// <summary>Which capability this turn targets.</summary>
    public InterviewCapability? TargetCapability { get; private set; }

    /// <summary>Lifecycle state of this individual turn.</summary>
    public InterviewTurnState TurnState { get; private set; }

    /// <summary>Verification status for progress tracking.</summary>
    public InterviewVerificationStatus VerificationStatus { get; private set; }

    // ── Audio and transcript fields (T05/T06) ──

    /// <summary>Storage key for the audio file (interviewer TTS or learner recording).</summary>
    public string AudioStorageKey { get; private set; }

    /// <summary>Audio duration in milliseconds.</summary>
    public int AudioDurationMs { get; private set; }

    /// <summary>Raw STT transcript (before learner confirmation).</summary>
    public string RawTranscript { get; private set; }

    /// <summary>Learner-confirmed transcript (used for answer evaluation).</summary>
    public string ConfirmedTranscript { get; private set; }

    /// <summary>Overall STT confidence score (0.0 to 1.0).</summary>
    public double TranscriptConfidence { get; private set; }

    /// <summary>Whether the learner edited the transcript before confirming.</summary>
    public bool LearnerEditedTranscript { get; private set; }

    // ── Rubric, decision, and evaluation fields (T01/T07/T08) ──

    /// <summary>JSON-serialized InterviewQuestionRubric for this turn.</summary>
    public string RubricJson { get; private set; }

    /// <summary>JSON-serialized InterviewTurnDecision metadata.</summary>
    public string DecisionJson { get; private set; }

    /// <summary>JSON-serialized pronunciation assessment report.</summary>
    public string PronunciationReportJson { get; private set; }

    /// <summary>JSON-serialized answer scorecard.</summary>
    public string ScorecardJson { get; private set; }

    // ── Factory methods ──

    /// <summary>Legacy factory — used by existing code. Will be migrated to new factory.</summary>
    public static InterviewTurn Create(
        string sessionId,
        InterviewTurnRole role,
        string message,
        int turnOrder,
        InterviewQuestionCategory? questionCategory = null,
        string audioUrl = "")
    {
        return new InterviewTurn
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = RequireNonEmpty(sessionId, nameof(sessionId)),
            Role = role,
            Message = message,
            AudioUrl = audioUrl ?? string.Empty,
            TurnOrder = turnOrder,
            QuestionCategory = questionCategory,
            TurnState = InterviewTurnState.Created,
            VerificationStatus = InterviewVerificationStatus.Unverified,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Create an interviewer turn with adaptive decision metadata.
    /// </summary>
    public static InterviewTurn CreateInterviewerTurn(
        string sessionId,
        string message,
        int turnOrder,
        InterviewTurnType turnType,
        InterviewCapability targetCapability,
        InterviewQuestionRubric? rubric,
        InterviewTurnDecision? decision,
        InterviewVerificationStatus verificationStatus)
    {
        return new InterviewTurn
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = RequireNonEmpty(sessionId, nameof(sessionId)),
            Role = InterviewTurnRole.Interviewer,
            Message = message,
            TurnOrder = turnOrder,
            TurnType = turnType,
            TargetCapability = targetCapability,
            QuestionCategory = MapTurnTypeToLegacyCategory(turnType),
            RubricJson = rubric is not null ? JsonSerializer.Serialize(rubric) : string.Empty,
            DecisionJson = decision is not null ? JsonSerializer.Serialize(decision) : string.Empty,
            TurnState = InterviewTurnState.Created,
            VerificationStatus = verificationStatus,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Create a learner turn (text-based, no audio yet).
    /// </summary>
    public static InterviewTurn CreateLearnerTurn(
        string sessionId,
        string message,
        int turnOrder,
        string audioUrl = "")
    {
        return new InterviewTurn
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = RequireNonEmpty(sessionId, nameof(sessionId)),
            Role = InterviewTurnRole.Learner,
            Message = message,
            AudioUrl = audioUrl ?? string.Empty,
            TurnOrder = turnOrder,
            TurnState = InterviewTurnState.Created,
            VerificationStatus = InterviewVerificationStatus.Unverified,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    // ── Turn state transitions ──

    public void MarkAudioReady(string audioStorageKey, int durationMs)
    {
        AudioStorageKey = audioStorageKey ?? string.Empty;
        AudioDurationMs = durationMs;
        TurnState = InterviewTurnState.AudioReady;
    }

    public void MarkLearnerAudioUploaded(string audioStorageKey, int durationMs)
    {
        if (Role != InterviewTurnRole.Learner)
            throw new InvalidOperationException("Only learner turns can have audio uploaded.");

        AudioStorageKey = audioStorageKey ?? string.Empty;
        AudioDurationMs = durationMs;
        TurnState = InterviewTurnState.LearnerAudioUploaded;
    }

    public void SetTranscript(string rawTranscript, double confidence)
    {
        if (Role != InterviewTurnRole.Learner)
            throw new InvalidOperationException("Only learner turns have transcripts.");

        RawTranscript = rawTranscript ?? string.Empty;
        TranscriptConfidence = confidence;
        TurnState = InterviewTurnState.TranscriptReady;
    }

    public void ConfirmTranscript(string confirmedTranscript, bool learnerEdited)
    {
        if (TurnState != InterviewTurnState.TranscriptReady)
            throw new InvalidOperationException($"Cannot confirm transcript in state {TurnState}.");

        ConfirmedTranscript = confirmedTranscript ?? string.Empty;
        LearnerEditedTranscript = learnerEdited;
        TurnState = InterviewTurnState.TranscriptConfirmed;
    }

    public void SetPronunciationReport(string pronunciationReportJson, InterviewVerificationStatus status)
    {
        PronunciationReportJson = pronunciationReportJson ?? string.Empty;
        VerificationStatus = status;
        TurnState = InterviewTurnState.PronunciationAssessed;
    }

    public void SetScorecard(string scorecardJson, InterviewVerificationStatus status)
    {
        ScorecardJson = scorecardJson ?? string.Empty;
        VerificationStatus = status;
        TurnState = InterviewTurnState.AnswerEvaluated;
    }

    public void Supersede()
    {
        TurnState = InterviewTurnState.Superseded;
    }

    // ── Helpers ──

    /// <summary>Get the text that should be used for answer evaluation.</summary>
    public string GetEvaluableTranscript()
    {
        if (!string.IsNullOrWhiteSpace(ConfirmedTranscript))
            return ConfirmedTranscript;
        if (!string.IsNullOrWhiteSpace(RawTranscript))
            return RawTranscript;
        return Message;
    }

    public InterviewQuestionRubric? GetRubric()
    {
        if (string.IsNullOrWhiteSpace(RubricJson))
            return null;
        return JsonSerializer.Deserialize<InterviewQuestionRubric>(RubricJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public InterviewTurnDecision? GetDecision()
    {
        if (string.IsNullOrWhiteSpace(DecisionJson))
            return null;
        return JsonSerializer.Deserialize<InterviewTurnDecision>(DecisionJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private static InterviewQuestionCategory MapTurnTypeToLegacyCategory(InterviewTurnType turnType)
    {
        return turnType switch
        {
            InterviewTurnType.OpeningQuestion => InterviewQuestionCategory.Opening,
            InterviewTurnType.MainQuestion => InterviewQuestionCategory.Technical,
            InterviewTurnType.FollowUp => InterviewQuestionCategory.FollowUp,
            InterviewTurnType.Clarification => InterviewQuestionCategory.FollowUp,
            InterviewTurnType.Challenge => InterviewQuestionCategory.Behavioral,
            InterviewTurnType.Transition => InterviewQuestionCategory.Situational,
            InterviewTurnType.Closing => InterviewQuestionCategory.Closing,
            _ => InterviewQuestionCategory.FollowUp
        };
    }

    private static string RequireNonEmpty(string value, string paramName) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", paramName) : value.Trim();
}
