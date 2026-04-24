using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EnglishCoach.Infrastructure.AI;

public class ProviderFailureHandler
{
    private readonly ILogger<ProviderFailureHandler> _logger;

    public ProviderFailureHandler(ILogger<ProviderFailureHandler> logger)
    {
        _logger = logger;
    }

    public ProviderFailureResult HandleTranscriptionFailure(
        TranscriptionResult result,
        Guid attemptId,
        string correlationId)
    {
        _logger.LogError(
            "Transcription failed for attempt {AttemptId}. CorrelationId: {CorrelationId}. Error: {ErrorCode} - {ErrorMessage}",
            attemptId,
            correlationId,
            result.ErrorCode,
            result.ErrorMessage
        );

        // Emit domain event for monitoring/alerting
        var domainEvent = new SpeechTranscriptionFailed(
            attemptId,
            result.Provider,
            result.ErrorCode,
            correlationId
        );

        // Determine if recoverable
        var isRecoverable = result.ErrorCode switch
        {
            "TIMEOUT" => true,
            "RATE_LIMITED" => true,
            "SERVICE_UNAVAILABLE" => true,
            "TRANSCRIPTION_FAILED" => false, // Non-recoverable
            _ => false
        };

        return new ProviderFailureResult(
            IsRecoverable: isRecoverable,
            ErrorCode: result.ErrorCode,
            ErrorMessage: result.ErrorMessage,
            DomainEvent: domainEvent
        );
    }

    public ProviderFailureResult HandleFeedbackFailure(
        FeedbackResult result,
        Guid attemptId,
        string correlationId)
    {
        _logger.LogError(
            "Feedback generation failed for attempt {AttemptId}. CorrelationId: {CorrelationId}. Error: {ErrorCode} - {ErrorMessage}",
            attemptId,
            correlationId,
            result.ErrorCode,
            result.ErrorMessage
        );

        var domainEvent = new FeedbackGenerationFailed(
            attemptId,
            result.Provider,
            result.ErrorCode,
            correlationId
        );

        var isRecoverable = result.ErrorCode switch
        {
            "TIMEOUT" => true,
            "RATE_LIMITED" => true,
            "SERVICE_UNAVAILABLE" => true,
            _ => false
        };

        return new ProviderFailureResult(
            IsRecoverable: isRecoverable,
            ErrorCode: result.ErrorCode,
            ErrorMessage: result.ErrorMessage,
            DomainEvent: domainEvent
        );
    }

    public ProviderFailureResult HandleRoleplayFailure(
        RoleplayResult result,
        Guid sessionId,
        string correlationId)
    {
        _logger.LogError(
            "Roleplay response failed for session {SessionId}. CorrelationId: {CorrelationId}. Error: {ErrorCode} - {ErrorMessage}",
            sessionId,
            correlationId,
            result.ErrorCode,
            result.ErrorMessage
        );

        var domainEvent = new RoleplayResponseFailed(
            sessionId,
            result.Provider,
            result.ErrorCode,
            correlationId
        );

        var isRecoverable = result.ErrorCode switch
        {
            "TIMEOUT" => true,
            "RATE_LIMITED" => true,
            "SERVICE_UNAVAILABLE" => true,
            _ => false
        };

        return new ProviderFailureResult(
            IsRecoverable: isRecoverable,
            ErrorCode: result.ErrorCode,
            ErrorMessage: result.ErrorMessage,
            DomainEvent: domainEvent
        );
    }

    public static string GenerateCorrelationId() => Guid.NewGuid().ToString("N")[..12].ToUpper();
}

public record ProviderFailureResult(
    bool IsRecoverable,
    string ErrorCode,
    string ErrorMessage,
    IDomainEvent DomainEvent
);

public interface IDomainEvent { }

public record SpeechTranscriptionFailed(
    Guid AttemptId,
    ProviderKind Provider,
    string ErrorCode,
    string CorrelationId
) : IDomainEvent;

public record FeedbackGenerationFailed(
    Guid AttemptId,
    ProviderKind Provider,
    string ErrorCode,
    string CorrelationId
) : IDomainEvent;

public record RoleplayResponseFailed(
    Guid SessionId,
    ProviderKind Provider,
    string ErrorCode,
    string CorrelationId
) : IDomainEvent;