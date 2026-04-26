using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.Roleplay;

namespace EnglishCoach.Infrastructure.AI.FakeAdapters;

public class FakeTranscriptionService : ISpeechTranscriptionService
{
    public ProviderKind Provider => ProviderKind.Fake;

    public Task<TranscriptionResult> TranscribeAsync(AudioReference audio, CancellationToken ct = default)
    {
        var transcript = $"[Fake transcript for attempt {audio.AttemptId}] " +
                         "Yesterday I completed the authentication module. " +
                         "Today I will work on the API integration. " +
                         "No blockers at the moment.";

        return Task.FromResult(TranscriptionResult.Success(transcript, ProviderKind.Fake));
    }
}

public class FakeFeedbackService : ISpeakingFeedbackService
{
    public ProviderKind Provider => ProviderKind.Fake;

    public Task<FeedbackResult> GenerateFeedbackAsync(
        SpeakingAttemptForEvaluation attempt,
        CancellationToken ct = default)
    {
        var content = new SpeakingFeedbackContent
        {
            PronunciationScore = "7.5/10",
            FluencyScore = "8/10",
            OverallFeedback = "Good attempt! Your sentence structure is clear and professional.",
            AreasToImprove = new[] { "Reduce filler words", "Practice past tense consistency" },
            Strengths = new[] { "Clear articulation", "Professional vocabulary" }
        };

        return Task.FromResult(FeedbackResult.Success(content, ProviderKind.Fake));
    }
}

public class FakeRoleplayService : IRoleplayResponseService
{
    public ProviderKind Provider => ProviderKind.Fake;

    public Task<RoleplayResult> GenerateResponseAsync(
        RoleplayContext context,
        CancellationToken ct = default)
    {
        var turnCount = context.ConversationHistory.Count;
        var isComplete = turnCount >= 6;

        var content = new RoleplayResponseContent
        {
            ClientMessage = isComplete
                ? "Great summary! I think we covered everything. Thank you for the update."
                : $"Thanks for the update. Can you tell me more about the progress on {context.ScenarioTitle}?",
            CoachingNote = isComplete
                ? "Good job! You maintained professional tone throughout."
                : "Try to be more specific about timelines when reporting progress.",
            IsSessionComplete = isComplete,
            EvaluatedCriteria = isComplete
                ? context.SuccessCriteria.ToArray()
                : Array.Empty<string>()
        };

        return Task.FromResult(RoleplayResult.Success(content, ProviderKind.Fake));
    }

    public Task<RoleplaySummary> EvaluateSessionAsync(RoleplayContext context, CancellationToken ct = default)
    {
        return Task.FromResult(new RoleplaySummary(
            "Passed",
            "Good job overall.",
            "Watch out for tenses.",
            "I'll get back to you.",
            "get back to you",
            "Be more confident."
        ));
    }
}
