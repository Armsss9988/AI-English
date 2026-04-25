using EnglishCoach.Application.Ports;
using Microsoft.Extensions.Logging;

namespace EnglishCoach.Infrastructure.AI;

public record ProviderFailureResult(
    bool IsRecoverable,
    string? ErrorCode,
    string? ErrorMessage);

public class ProviderFailureHandler
{
    private readonly ILogger<ProviderFailureHandler> _logger;

    public ProviderFailureHandler(ILogger<ProviderFailureHandler> logger)
    {
        _logger = logger;
    }

    public ProviderFailureResult HandleTranscriptionFailure(
        TranscriptionResult result,
        string attemptId,
        string correlationId)
    {
        _logger.LogError(
            "Transcription failed for attempt {AttemptId}. CorrelationId: {CorrelationId}. Error: {ErrorCode} - {ErrorMessage}",
            attemptId, correlationId, result.ErrorCode, result.ErrorMessage);

        var isRecoverable = IsRecoverableError(result.ErrorCode);

        return new ProviderFailureResult(
            IsRecoverable: isRecoverable,
            ErrorCode: result.ErrorCode,
            ErrorMessage: result.ErrorMessage);
    }

    public ProviderFailureResult HandleFeedbackFailure(
        FeedbackResult result,
        string attemptId,
        string correlationId)
    {
        _logger.LogError(
            "Feedback failed for attempt {AttemptId}. CorrelationId: {CorrelationId}. Error: {ErrorCode} - {ErrorMessage}",
            attemptId, correlationId, result.ErrorCode, result.ErrorMessage);

        var isRecoverable = IsRecoverableError(result.ErrorCode);

        return new ProviderFailureResult(
            IsRecoverable: isRecoverable,
            ErrorCode: result.ErrorCode,
            ErrorMessage: result.ErrorMessage);
    }

    public ProviderFailureResult HandleRoleplayFailure(
        RoleplayResult result,
        string sessionId,
        string correlationId)
    {
        _logger.LogError(
            "Roleplay response failed for session {SessionId}. CorrelationId: {CorrelationId}. Error: {ErrorCode} - {ErrorMessage}",
            sessionId, correlationId, result.ErrorCode, result.ErrorMessage);

        var isRecoverable = IsRecoverableError(result.ErrorCode);

        return new ProviderFailureResult(
            IsRecoverable: isRecoverable,
            ErrorCode: result.ErrorCode,
            ErrorMessage: result.ErrorMessage);
    }

    private static bool IsRecoverableError(string? errorCode) => errorCode switch
    {
        "TIMEOUT" => true,
        "RATE_LIMITED" => true,
        "SERVICE_UNAVAILABLE" => true,
        "TRANSCRIPTION_FAILED" => false,
        "FEEDBACK_FAILED" => false,
        "ROLEPLAY_FAILED" => false,
        _ => false
    };
}
