using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.Speaking;
using Microsoft.Extensions.Logging;

namespace EnglishCoach.Infrastructure.Jobs;

public class SpeakingEvaluationJob
{
    private readonly ISpeechTranscriptionService _transcriptionService;
    private readonly ISpeakingFeedbackService _feedbackService;
    private readonly ISpeakingAttemptRepository _attemptRepository;
    private readonly ISubmitSpeakingEvaluationUseCase _submitEvaluationUseCase;
    private readonly ILogger<SpeakingEvaluationJob> _logger;

    public SpeakingEvaluationJob(
        ISpeechTranscriptionService transcriptionService,
        ISpeakingFeedbackService feedbackService,
        ISpeakingAttemptRepository attemptRepository,
        ISubmitSpeakingEvaluationUseCase submitEvaluationUseCase,
        ILogger<SpeakingEvaluationJob> logger)
    {
        _transcriptionService = transcriptionService;
        _feedbackService = feedbackService;
        _attemptRepository = attemptRepository;
        _submitEvaluationUseCase = submitEvaluationUseCase;
        _logger = logger;
    }

    public async Task ProcessAsync(Guid attemptId, CancellationToken ct = default)
    {
        var attempt = await _attemptRepository.GetByIdAsync(attemptId, ct);
        if (attempt == null)
        {
            _logger.LogWarning("Attempt {AttemptId} not found", attemptId);
            return;
        }

        if (attempt.State == SpeakingAttemptState.Evaluated)
        {
            _logger.LogInformation("Attempt {AttemptId} already evaluated, skipping", attemptId);
            return; // Idempotent - already processed
        }

        try
        {
            // Step 1: Transcribe
            attempt.MarkTranscribing();

            var transcriptionResult = await _transcriptionService.TranscribeAsync(
                new AudioReference(attemptId, attempt.AudioUrl),
                ct);

            if (!transcriptionResult.IsSuccess)
            {
                attempt.MarkEvaluationFailed(transcriptionResult.ErrorCode, transcriptionResult.ErrorMessage);
                await _attemptRepository.UpdateAsync(attempt, ct);
                _logger.LogError("Transcription failed for attempt {AttemptId}: {Error}", attemptId, transcriptionResult.ErrorMessage);
                return;
            }

            attempt.SetTranscript(transcriptionResult.Transcript!);

            // Step 2: Generate Feedback
            attempt.MarkEvaluating();

            var attemptForEvaluation = new SpeakingAttemptForEvaluation(
                attempt.Id,
                attempt.SessionId,
                attempt.Transcript!);

            var feedbackResult = await _feedbackService.GenerateFeedbackAsync(attemptForEvaluation, ct);

            if (!feedbackResult.IsSuccess)
            {
                attempt.MarkEvaluationFailed(feedbackResult.ErrorCode, feedbackResult.ErrorMessage);
                await _attemptRepository.UpdateAsync(attempt, ct);
                _logger.LogError("Feedback generation failed for attempt {AttemptId}: {Error}", attemptId, feedbackResult.ErrorMessage);
                return;
            }

            // Step 3: Submit to use case (which handles business logic)
            await _submitEvaluationUseCase.ExecuteAsync(
                attemptId,
                transcriptionResult.Transcript!,
                feedbackResult.Content!,
                ct);

            attempt.MarkEvaluated();
            await _attemptRepository.UpdateAsync(attempt, ct);

            _logger.LogInformation("Successfully processed attempt {AttemptId}", attemptId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing attempt {AttemptId}", attemptId);
            attempt.MarkEvaluationFailed("INTERNAL_ERROR", ex.Message);
            await _attemptRepository.UpdateAsync(attempt, ct);
            throw;
        }
    }

    public async Task ProcessWithRetryAsync(Guid attemptId, int maxRetries = 3, CancellationToken ct = default)
    {
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                await ProcessAsync(attemptId, ct);
                return;
            }
            catch (Exception ex) when (retryCount < maxRetries - 1)
            {
                retryCount++;
                _logger.LogWarning(ex, "Retry {RetryCount}/{MaxRetries} for attempt {AttemptId}", retryCount, maxRetries, attemptId);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), ct);
            }
        }
    }
}

public interface ISpeakingAttemptRepository
{
    Task<SpeakingAttemptEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(SpeakingAttemptEntity attempt, CancellationToken ct = default);
}

public class SpeakingAttemptEntity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public SpeakingAttemptState State { get; set; }
    public string? Transcript { get; set; }
    public string? Feedback { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public void MarkTranscribing() => State = SpeakingAttemptState.Transcribed;
    public void MarkEvaluating() => State = SpeakingAttemptState.Evaluating;

    public void MarkEvaluated()
    {
        State = SpeakingAttemptState.Evaluated;
        ErrorCode = null;
        ErrorMessage = null;
    }

    public void MarkEvaluationFailed(string errorCode, string errorMessage)
    {
        State = SpeakingAttemptState.EvaluationFailed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public void SetTranscript(string transcript)
    {
        Transcript = transcript;
    }
}

public interface ISubmitSpeakingEvaluationUseCase
{
    Task ExecuteAsync(Guid attemptId, string transcript, SpeakingFeedbackContent feedback, CancellationToken ct = default);
}