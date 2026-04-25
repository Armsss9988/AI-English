using EnglishCoach.Application.Ports;
using EnglishCoach.Application.Speaking;
using EnglishCoach.Domain.Speaking;
using Microsoft.Extensions.Logging;

namespace EnglishCoach.Infrastructure.Jobs;

/// <summary>
/// Background job that processes speech evaluation pipeline:
/// Transcribe audio → Mark transcribed → Delegate to SubmitSpeakingAttemptEvaluationUseCase.
/// Idempotent: already-evaluated attempts are skipped.
/// </summary>
public class SpeakingEvaluationJob
{
    private readonly ISpeechTranscriptionService _transcriptionService;
    private readonly ISpeakingAttemptRepository _attemptRepository;
    private readonly SubmitSpeakingAttemptEvaluationUseCase _submitEvaluationUseCase;
    private readonly ILogger<SpeakingEvaluationJob> _logger;

    public SpeakingEvaluationJob(
        ISpeechTranscriptionService transcriptionService,
        ISpeakingAttemptRepository attemptRepository,
        SubmitSpeakingAttemptEvaluationUseCase submitEvaluationUseCase,
        ILogger<SpeakingEvaluationJob> logger)
    {
        _transcriptionService = transcriptionService;
        _attemptRepository = attemptRepository;
        _submitEvaluationUseCase = submitEvaluationUseCase;
        _logger = logger;
    }

    public async Task ProcessAsync(string attemptId, string learnerId, CancellationToken ct = default)
    {
        var attempt = await _attemptRepository.GetByIdAsync(attemptId, ct);
        if (attempt == null)
        {
            _logger.LogWarning("Attempt {AttemptId} not found", attemptId);
            return;
        }

        if (attempt.State == SpeakingAttemptState.Evaluated ||
            attempt.State == SpeakingAttemptState.Finalized)
        {
            _logger.LogInformation("Attempt {AttemptId} already evaluated, skipping", attemptId);
            return; // Idempotent — already processed
        }

        try
        {
            // Step 1: Transcribe
            var transcriptionResult = await _transcriptionService.TranscribeAsync(
                new AudioReference(Guid.Parse(attemptId), attempt.AudioUrl ?? string.Empty), ct);

            if (!transcriptionResult.IsSuccess)
            {
                _logger.LogError("Transcription failed for attempt {AttemptId}: {Error}",
                    attemptId, transcriptionResult.ErrorMessage);
                return;
            }

            // Step 2: Mark transcribed (raw + normalized)
            attempt.MarkTranscribed(transcriptionResult.Transcript!, transcriptionResult.Transcript!);
            await _attemptRepository.UpdateAsync(attempt, ct);

            // Step 3: Delegate to use case for feedback + evaluation
            await _submitEvaluationUseCase.ExecuteAsync(learnerId, Guid.Parse(attemptId), ct);

            _logger.LogInformation("Speaking evaluation completed for attempt {AttemptId}", attemptId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error evaluating attempt {AttemptId}", attemptId);
            throw;
        }
    }
}
