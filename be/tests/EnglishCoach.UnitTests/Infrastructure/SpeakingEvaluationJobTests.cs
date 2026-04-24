using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.Speaking;
using EnglishCoach.Infrastructure.AI.FakeAdapters;
using EnglishCoach.Infrastructure.Jobs;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EnglishCoach.UnitTests.Infrastructure;

public class SpeakingEvaluationJobTests
{
    private readonly Mock<ISpeakingAttemptRepository> _mockRepository;
    private readonly Mock<ISubmitSpeakingEvaluationUseCase> _mockUseCase;
    private readonly Mock<ILogger<SpeakingEvaluationJob>> _mockLogger;
    private readonly SpeakingEvaluationJob _job;

    public SpeakingEvaluationJobTests()
    {
        _mockRepository = new Mock<ISpeakingAttemptRepository>();
        _mockUseCase = new Mock<ISubmitSpeakingEvaluationUseCase>();
        _mockLogger = new Mock<ILogger<SpeakingEvaluationJob>>();

        var transcriptionService = new FakeTranscriptionService("Test transcript");
        var feedbackService = new FakeFeedbackService(new SpeakingFeedbackContent
        {
            PronunciationScore = "90",
            FluencyScore = "85",
            OverallFeedback = "Good",
            AreasToImprove = Array.Empty<string>()
        });

        _job = new SpeakingEvaluationJob(
            transcriptionService,
            feedbackService,
            _mockRepository.Object,
            _mockUseCase.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task ProcessAsync_ShouldSkip_WhenAttemptAlreadyEvaluated()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var attempt = CreateAttempt(attemptId, SpeakingAttemptState.Evaluated);
        _mockRepository.Setup(x => x.GetByIdAsync(attemptId, default)).ReturnsAsync(attempt);

        // Act
        await _job.ProcessAsync(attemptId);

        // Assert
        _mockUseCase.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<SpeakingFeedbackContent>(), default), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ShouldBeIdempotent_WhenCalledTwice()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var attempt = CreateAttempt(attemptId, SpeakingAttemptState.Created);
        _mockRepository.Setup(x => x.GetByIdAsync(attemptId, default))
            .ReturnsAsync(() => attempt);

        // Act - call twice
        await _job.ProcessAsync(attemptId);
        await _job.ProcessAsync(attemptId);

        // Assert - use case should only be called once (first call marks as evaluated)
        _mockUseCase.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<SpeakingFeedbackContent>(), default), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallUseCase_WhenTranscriptionAndFeedbackSucceed()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var attempt = CreateAttempt(attemptId, SpeakingAttemptState.Uploaded);
        _mockRepository.Setup(x => x.GetByIdAsync(attemptId, default)).ReturnsAsync(attempt);

        // Act
        await _job.ProcessAsync(attemptId);

        // Assert
        _mockUseCase.Verify(x => x.ExecuteAsync(
            attemptId,
            "Test transcript",
            It.Is<SpeakingFeedbackContent>(f => f.PronunciationScore == "90"),
            default
        ), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldMarkFailed_WhenTranscriptionFails()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var attempt = CreateAttempt(attemptId, SpeakingAttemptState.Uploaded);
        _mockRepository.Setup(x => x.GetByIdAsync(attemptId, default)).ReturnsAsync(attempt);

        var failingJob = new SpeakingEvaluationJob(
            FakeTranscriptionService.Failure("TIMEOUT", "Request timed out"),
            new FakeFeedbackService(),
            _mockRepository.Object,
            _mockUseCase.Object,
            _mockLogger.Object
        );

        // Act
        await failingJob.ProcessAsync(attemptId);

        // Assert
        attempt.State.Should().Be(SpeakingAttemptState.EvaluationFailed);
        attempt.ErrorCode.Should().Be("TIMEOUT");
        _mockUseCase.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<SpeakingFeedbackContent>(), default), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ShouldNotUpdateReviewDirectly()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var attempt = CreateAttempt(attemptId, SpeakingAttemptState.Uploaded);
        _mockRepository.Setup(x => x.GetByIdAsync(attemptId, default)).ReturnsAsync(attempt);

        // Act
        await _job.ProcessAsync(attemptId);

        // Assert - job should NOT update review/progress directly, only through use case
        _mockUseCase.Verify(x => x.ExecuteAsync(
            attemptId,
            It.IsAny<string>(),
            It.IsAny<SpeakingFeedbackContent>(),
            default
        ), Times.Once);
    }

    private static SpeakingAttemptEntity CreateAttempt(Guid id, SpeakingAttemptState state)
    {
        return new SpeakingAttemptEntity
        {
            Id = id,
            SessionId = Guid.NewGuid(),
            AudioUrl = "https://example.com/audio.mp3",
            State = state,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}