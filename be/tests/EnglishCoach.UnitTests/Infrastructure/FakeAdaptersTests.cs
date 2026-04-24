using EnglishCoach.Application.Ports;
using EnglishCoach.Infrastructure.AI.FakeAdapters;
using FluentAssertions;
using Xunit;

namespace EnglishCoach.UnitTests.Infrastructure;

public class FakeTranscriptionServiceTests
{
    [Fact]
    public async Task TranscribeAsync_ShouldReturnSuccess_WhenNotConfiguredToFail()
    {
        // Arrange
        var service = FakeTranscriptionService.Success("Hello, how are you?");
        var audio = new AudioReference(Guid.NewGuid(), "https://example.com/audio.mp3");

        // Act
        var result = await service.TranscribeAsync(audio);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Transcript.Should().Be("Hello, how are you?");
        result.Provider.Should().Be(ProviderKind.Fake);
        result.ErrorCode.Should().BeEmpty();
    }

    [Fact]
    public async Task TranscribeAsync_ShouldReturnFailure_WhenConfiguredToFail()
    {
        // Arrange
        var service = FakeTranscriptionService.Failure("TIMEOUT", "Request timed out");
        var audio = new AudioReference(Guid.NewGuid(), "https://example.com/audio.mp3");

        // Act
        var result = await service.TranscribeAsync(audio);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Transcript.Should().BeNull();
        result.ErrorCode.Should().Be("TIMEOUT");
        result.ErrorMessage.Should().Be("Request timed out");
        result.Provider.Should().Be(ProviderKind.Fake);
    }

    [Fact]
    public void Provider_ShouldBeFake()
    {
        var service = new FakeTranscriptionService();
        service.Provider.Should().Be(ProviderKind.Fake);
    }
}

public class FakeFeedbackServiceTests
{
    [Fact]
    public async Task GenerateFeedbackAsync_ShouldReturnSuccess_WhenNotConfiguredToFail()
    {
        // Arrange
        var customFeedback = new SpeakingFeedbackContent
        {
            PronunciationScore = "90",
            FluencyScore = "85",
            OverallFeedback = "Excellent work",
            AreasToImprove = new[] { "Minor pronunciation issues" },
            Strengths = new[] { "Good fluency" }
        };
        var service = FakeFeedbackService.Success(customFeedback);
        var attempt = new SpeakingAttemptForEvaluation(Guid.NewGuid(), Guid.NewGuid(), "Test transcript");

        // Act
        var result = await service.GenerateFeedbackAsync(attempt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Content.Should().NotBeNull();
        result.Content!.PronunciationScore.Should().Be("90");
        result.Provider.Should().Be(ProviderKind.Fake);
    }

    [Fact]
    public async Task GenerateFeedbackAsync_ShouldReturnDefaultFeedback_WhenNotProvided()
    {
        // Arrange
        var service = FakeFeedbackService.Success();
        var attempt = new SpeakingAttemptForEvaluation(Guid.NewGuid(), Guid.NewGuid(), "Test transcript");

        // Act
        var result = await service.GenerateFeedbackAsync(attempt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Content.Should().NotBeNull();
        result.Content!.PronunciationScore.Should().Be("85");
    }

    [Fact]
    public async Task GenerateFeedbackAsync_ShouldReturnFailure_WhenConfiguredToFail()
    {
        // Arrange
        var service = FakeFeedbackService.Failure("RATE_LIMITED", "Rate limit exceeded");
        var attempt = new SpeakingAttemptForEvaluation(Guid.NewGuid(), Guid.NewGuid(), "Test transcript");

        // Act
        var result = await service.GenerateFeedbackAsync(attempt);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Content.Should().BeNull();
        result.ErrorCode.Should().Be("RATE_LIMITED");
    }
}

public class FakeRoleplayServiceTests
{
    [Fact]
    public async Task GenerateResponseAsync_ShouldReturnSuccess_WhenNotConfiguredToFail()
    {
        // Arrange
        var customResponse = new RoleplayResponseContent
        {
            ClientMessage = "Thanks for the update",
            CoachingNote = "Good job",
            IsSessionComplete = false,
            EvaluatedCriteria = new[] { "Clarity" }
        };
        var service = FakeRoleplayService.Success(customResponse);
        var context = CreateTestContext();

        // Act
        var result = await service.GenerateResponseAsync(context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Content.Should().NotBeNull();
        result.Content!.ClientMessage.Should().Be("Thanks for the update");
        result.Provider.Should().Be(ProviderKind.Fake);
    }

    [Fact]
    public async Task GenerateResponseAsync_ShouldReturnDefaultResponse_WhenNotProvided()
    {
        // Arrange
        var service = FakeRoleplayService.Success();
        var context = CreateTestContext();

        // Act
        var result = await service.GenerateResponseAsync(context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Content.Should().NotBeNull();
        result.Content!.ClientMessage.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateResponseAsync_ShouldReturnFailure_WhenConfiguredToFail()
    {
        // Arrange
        var service = FakeRoleplayService.Failure("SERVICE_UNAVAILABLE", "Service temporarily unavailable");
        var context = CreateTestContext();

        // Act
        var result = await service.GenerateResponseAsync(context);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Content.Should().BeNull();
        result.ErrorCode.Should().Be("SERVICE_UNAVAILABLE");
    }

    [Fact]
    public void Provider_ShouldBeFake()
    {
        var service = new FakeRoleplayService();
        service.Provider.Should().Be(ProviderKind.Fake);
    }

    private static RoleplayContext CreateTestContext() => new()
    {
        SessionId = Guid.NewGuid(),
        ScenarioId = Guid.NewGuid(),
        ScenarioTitle = "Daily Standup",
        ScenarioPersona = "Client PM",
        ScenarioGoal = "Give daily update",
        Difficulty = 2,
        ConversationHistory = new[]
        {
            new RoleplayTurn { Speaker = "AI", Message = "Good morning", Timestamp = DateTimeOffset.UtcNow }
        },
        LatestLearnerTurn = new RoleplayTurn
        {
            Speaker = "Learner",
            Message = "I completed the authentication module",
            Timestamp = DateTimeOffset.UtcNow
        },
        SuccessCriteria = new[] { "Clarity", "Completeness" }
    };
}