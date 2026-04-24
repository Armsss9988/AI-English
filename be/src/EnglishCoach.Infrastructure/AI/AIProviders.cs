using EnglishCoach.Application.Ports;

namespace EnglishCoach.Infrastructure.AI;

public class OpenAiTranscriptionService : ISpeechTranscriptionService
{
    public async Task<TranscriptionResult> TranscribeAsync(AudioReference audio, CancellationToken ct = default)
    {
        // TODO: Implement OpenAI transcription
        // This is a placeholder that returns a fake result for development
        await Task.Delay(100, ct);
        return new TranscriptionResult(
            IsSuccess: true,
            Transcript: "This is a sample transcription.",
            ErrorMessage: null,
            Provider: "OpenAI"
        );
    }
}

public class OpenAiFeedbackService : ISpeakingFeedbackService
{
    public async Task<FeedbackResult> GenerateFeedbackAsync(
        SpeakingAttempt attempt,
        CancellationToken ct = default)
    {
        // TODO: Implement OpenAI feedback generation
        await Task.Delay(100, ct);
        return new FeedbackResult(
            IsSuccess: true,
            Content: new SpeakingFeedbackContent(
                PronunciationScore: "85",
                FluencyScore: "80",
                OverallFeedback: "Good attempt. Work on your pronunciation of 'client'.",
                AreasToImprove: new[] { "Practice 'th' sounds", "Work on word stress" }
            ),
            ErrorMessage: null,
            Provider: "OpenAI"
        );
    }
}

public class OpenAiRoleplayService : IRoleplayResponseService
{
    public async Task<RoleplayResult> GenerateResponseAsync(
        RoleplayContext context,
        CancellationToken ct = default)
    {
        // TODO: Implement OpenAI roleplay response
        await Task.Delay(100, ct);
        return new RoleplayResult(
            IsSuccess: true,
            Content: new RoleplayResponseContent(
                ClientMessage: "Thanks for the update! Let me review the issues you mentioned.",
                CoachingNote: "Good use of technical vocabulary.",
                IsSessionComplete: false
            ),
            ErrorMessage: null,
            Provider: "OpenAI"
        );
    }
}