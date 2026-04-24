using EnglishCoach.Domain.Speaking;
using EnglishCoach.Application.Ports;

namespace EnglishCoach.Application.Speaking;

public sealed class SubmitSpeakingAttemptEvaluationUseCase
{
    private readonly ISpeakingAttemptRepository _attemptRepository;
    private readonly ISpeakingFeedbackService _feedbackService;

    public SubmitSpeakingAttemptEvaluationUseCase(
        ISpeakingAttemptRepository attemptRepository,
        ISpeakingFeedbackService feedbackService)
    {
        _attemptRepository = attemptRepository;
        _feedbackService = feedbackService;
    }

    public async Task<SpeakingFeedback> ExecuteAsync(
        string learnerId,
        Guid attemptId,
        CancellationToken ct = default)
    {
        var attempt = await _attemptRepository.GetByIdAsync(attemptId.ToString("N"), ct);
        if (attempt is null || attempt.LearnerId != learnerId)
        {
            throw new InvalidOperationException("Attempt not found or access denied.");
        }

        if (attempt.State != SpeakingAttemptState.Transcribed)
        {
            throw new InvalidOperationException("Attempt must be transcribed before evaluation.");
        }

        var attemptForEvaluation = new SpeakingAttemptForEvaluation(
            Guid.Parse(attempt.Id),
            Guid.Empty, // SessionId is not applicable for individual drill attempt
            attempt.NormalizedTranscript
        );

        var feedbackResult = await _feedbackService.GenerateFeedbackAsync(attemptForEvaluation, ct);
        
        if (!feedbackResult.IsSuccess || feedbackResult.Content is null)
        {
            throw new InvalidOperationException("Feedback generation failed: " + feedbackResult.ErrorMessage);
        }

        var domainFeedback = new SpeakingFeedback(
            string.Join(", ", feedbackResult.Content.AreasToImprove), // TopMistakes
            "", // ImprovedAnswer - not mapped in the interface, we'll leave empty or map from overall
            string.Join(", ", feedbackResult.Content.Strengths), // PhrasesToReview
            feedbackResult.Content.OverallFeedback // RetryPrompt
        );

        attempt.MarkEvaluated(domainFeedback);
        attempt.FinalizeAttempt();

        await _attemptRepository.UpdateAsync(attempt, ct);

        // Emits SpeakingAttemptEvaluated event (infrastructure concern)

        return domainFeedback;
    }
}
