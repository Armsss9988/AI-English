namespace EnglishCoach.Domain.InterviewPractice;

/// <summary>
/// Tracks the lifecycle of an individual interview turn.
/// Learner turns progress through audio upload, transcription, confirmation, assessment, and evaluation.
/// Interviewer turns progress through creation and audio generation.
/// </summary>
public enum InterviewTurnState
{
    /// <summary>Turn has been created with text content.</summary>
    Created,

    /// <summary>Audio has been generated for this turn (interviewer) or is ready to play.</summary>
    AudioReady,

    /// <summary>Learner audio has been uploaded to storage.</summary>
    LearnerAudioUploaded,

    /// <summary>STT transcript is available for review.</summary>
    TranscriptReady,

    /// <summary>Learner has confirmed (or edited) the transcript.</summary>
    TranscriptConfirmed,

    /// <summary>Pronunciation assessment has been completed for this turn.</summary>
    PronunciationAssessed,

    /// <summary>Answer has been evaluated with a scorecard.</summary>
    AnswerEvaluated,

    /// <summary>This turn has been superseded by a retry attempt.</summary>
    Superseded
}
