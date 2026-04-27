using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.InterviewPractice;
using EnglishCoach.Infrastructure.AI.FakeAdapters;
using Xunit;

namespace EnglishCoach.UnitTests.InterviewPractice;

public sealed class FakeAdaptiveInterviewerTests
{
    private readonly FakeAdaptiveInterviewerService _service = new();

    [Fact]
    public async Task NoLearnerAnswer_ReturnsOpeningQuestion()
    {
        var context = CreateContext(latestTranscript: null);
        var result = await _service.GenerateInterviewerTurnAsync(context);

        Assert.True(result.IsSuccess);
        Assert.Equal(InterviewTurnType.OpeningQuestion, result.TurnType);
        Assert.Equal("no_learner_answer", result.ReasonCode);
    }

    [Fact]
    public async Task ShortAnswer_ReturnsFollowUp()
    {
        var context = CreateContext(latestTranscript: "I work with C#.");
        var result = await _service.GenerateInterviewerTurnAsync(context);

        Assert.True(result.IsSuccess);
        Assert.Equal(InterviewTurnType.FollowUp, result.TurnType);
        Assert.Equal("shallow_answer", result.ReasonCode);
        Assert.False(result.ShouldAdvancePlan);
    }

    [Fact]
    public async Task SpecificAnswer_CanAdvancePlan()
    {
        var longAnswer = string.Join(" ", Enumerable.Repeat("I have extensive experience working with", 10));
        var context = CreateContext(latestTranscript: longAnswer);
        var result = await _service.GenerateInterviewerTurnAsync(context);

        Assert.True(result.IsSuccess);
        Assert.Equal(InterviewTurnType.MainQuestion, result.TurnType);
        Assert.True(result.ShouldAdvancePlan);
    }

    [Fact]
    public async Task TrainingMode_IncludesHint()
    {
        var context = CreateContext(
            latestTranscript: null,
            mode: InterviewMode.TrainingInterview);
        var result = await _service.GenerateInterviewerTurnAsync(context);

        Assert.NotNull(result.LearnerFacingHint);
    }

    [Fact]
    public async Task RealInterviewMode_NoHintForLongAnswer()
    {
        var longAnswer = string.Join(" ", Enumerable.Repeat("I have extensive experience working with", 10));
        var context = CreateContext(
            latestTranscript: longAnswer,
            mode: InterviewMode.RealInterview);
        var result = await _service.GenerateInterviewerTurnAsync(context);

        Assert.Null(result.LearnerFacingHint);
    }

    [Fact]
    public async Task EvaluateAnswer_ShortAnswer_RequiresRetry()
    {
        var context = new AnswerEvaluationContext
        {
            ConfirmedTranscript = "I do backend",
            TargetCapability = InterviewCapability.ProjectDeepDive
        };
        var result = await _service.EvaluateAnswerAsync(context);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Scorecard);
        Assert.True(result.Scorecard.RequiresRetry);
    }

    [Fact]
    public async Task EvaluateAnswer_LongAnswer_NoRetry()
    {
        var longAnswer = string.Join(" ", Enumerable.Repeat("detailed experience working with", 15));
        var context = new AnswerEvaluationContext
        {
            ConfirmedTranscript = longAnswer,
            TargetCapability = InterviewCapability.ProjectDeepDive
        };
        var result = await _service.EvaluateAnswerAsync(context);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Scorecard);
        Assert.False(result.Scorecard.RequiresRetry);
    }

    [Fact]
    public async Task EvaluateAnswer_IncludesCorrections()
    {
        var context = new AnswerEvaluationContext
        {
            ConfirmedTranscript = "I do the backend and handle database",
            TargetCapability = InterviewCapability.TechnicalTradeoff
        };
        var result = await _service.EvaluateAnswerAsync(context);

        Assert.NotEmpty(result.Scorecard!.Corrections);
        Assert.NotEmpty(result.Scorecard.Corrections[0].ExplanationVi);
    }

    private static InterviewTurnGenerationContext CreateContext(
        string? latestTranscript = null,
        InterviewMode mode = InterviewMode.TrainingInterview)
    {
        return new InterviewTurnGenerationContext
        {
            SessionId = "test-session",
            InterviewMode = mode,
            CvAnalysis = "test cv",
            JdAnalysis = "test jd",
            InterviewPlan = "test plan",
            CurrentQuestionNumber = 1,
            PlannedQuestionCount = 5,
            LatestLearnerTranscript = latestTranscript
        };
    }
}

public sealed class FakePronunciationTests
{
    private readonly FakePronunciationAssessmentService _service = new();

    [Fact]
    public async Task AspNetCore_FlagsTechnicalTerm()
    {
        var request = new PronunciationAssessmentRequest
        {
            ConfirmedTranscript = "I work with ASP.NET Core",
            RawTranscript = "I work with asp net core"
        };
        var result = await _service.AssessAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.WordIssues, i => i.IssueType == "TechnicalTerm");
    }

    [Fact]
    public async Task LowConfidenceWords_FlaggedAsPronunciationIssues()
    {
        var request = new PronunciationAssessmentRequest
        {
            ConfirmedTranscript = "test transcript",
            RawTranscript = "test transcript",
            WordConfidences = new[]
            {
                new WordTiming { Word = "test", Confidence = 0.5, StartMs = 0, EndMs = 300 },
                new WordTiming { Word = "transcript", Confidence = 0.9, StartMs = 300, EndMs = 700 }
            }
        };
        var result = await _service.AssessAsync(request);

        Assert.Contains(result.WordIssues, i => i.IssueType == "LowConfidenceWord" && i.HeardAs == "test");
    }
}

public sealed class FakeTtsAndSttTests
{
    [Fact]
    public async Task FakeTts_ReturnsAudioData()
    {
        var service = new FakeTextToSpeechService();
        var result = await service.SynthesizeAsync(new TextToSpeechRequest
        {
            Text = "Tell me about yourself."
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.AudioData);
        Assert.True(result.AudioData!.Length > 0);
        Assert.True(result.DurationMs > 0);
    }

    [Fact]
    public async Task FakeStt_ReturnsTranscriptWithWordTimings()
    {
        var service = new FakeSpeechToTextService();
        var result = await service.TranscribeAsync(new SpeechToTextRequest
        {
            AudioStorageKey = "test/audio.webm"
        });

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Transcript));
        Assert.True(result.Confidence > 0.5);
        Assert.NotNull(result.WordTimings);
        Assert.NotEmpty(result.WordTimings);
        Assert.True(result.UsedFallback);
    }
}
