namespace EnglishCoach.SharedKernel.Errors;

public enum ErrorCode
{
    None = 0,

    // General
    NotFound,
    ValidationError,
    Forbidden,
    Conflict,
    InternalError,
    DomainError,

    // Speaking
    TranscriptionFailed,
    TranscriptionTimeout,
    FeedbackGenerationFailed,
    AudioUploadFailed,
    SpeakingAttemptNotFound,
    InvalidSpeakingState,

    // Roleplay
    RoleplaySessionNotFound,
    InvalidRoleplayState,
    SessionAlreadyFinalized,
    NoTurnsRecorded,

    // Review
    ReviewItemNotFound,
    InvalidReviewState,
    ReviewNotDue,

    // Content
    ContentNotFound,
    ContentNotPublished,
    CannotEditPublishedContent,
    InvalidContentStateTransition,

    // Progress
    ReadinessSnapshotNotFound,
    InvalidFormulaVersion,

    // Provider
    ProviderUnavailable,
    ProviderTimeout,
    ProviderRateLimited,
    ProviderInvalidResponse,

    // Auth
    Unauthorized,
    TokenExpired,
    InvalidToken,
}