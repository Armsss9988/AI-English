using EnglishCoach.Application.Ports;

namespace EnglishCoach.Infrastructure.AI.FakeAdapters;

public class FakeFeedbackService : ISpeakingFeedbackService
{
    public ProviderKind Provider => ProviderKind.Fake;

    private readonly SpeakingFeedbackContent _feedback;
    private readonly bool _shouldFail;
    private readonly string _errorCode;
    private readonly string _errorMessage;

    public FakeFeedbackService(
        SpeakingFeedbackContent? feedback = null,
        bool shouldFail = false,
        string errorCode = "FEEDBACK_GENERATION_FAILED",
        string errorMessage = "Simulated feedback failure")
    {
        _feedback = feedback ?? CreateDefaultFeedback();
        _shouldFail = shouldFail;
        _errorCode = errorCode;
        _errorMessage = errorMessage;
    }

    public Task<FeedbackResult> GenerateFeedbackAsync(
        SpeakingAttemptForEvaluation attempt,
        CancellationToken ct = default)
    {
        if (_shouldFail)
        {
            return Task.FromResult(FeedbackResult.Failure(_errorCode, _errorMessage, Provider));
        }

        return Task.FromResult(FeedbackResult.Success(_feedback, Provider));
    }

    private static SpeakingFeedbackContent CreateDefaultFeedback() => new()
    {
        PronunciationScore = "85",
        FluencyScore = "80",
        OverallFeedback = "Good attempt. Work on word stress and intonation.",
        AreasToImprove = new[] { "Word stress", "Intonation", "Connecting words" },
        Strengths = new[] { "Clear pronunciation", "Good vocabulary usage" }
    };

    public static FakeFeedbackService Success(SpeakingFeedbackContent? feedback = null)
        => new(feedback);

    public static FakeFeedbackService Failure(string errorCode = "FEEDBACK_GENERATION_FAILED", string errorMessage = "Simulated failure")
        => new(shouldFail: true, errorCode: errorCode, errorMessage: errorMessage);
}