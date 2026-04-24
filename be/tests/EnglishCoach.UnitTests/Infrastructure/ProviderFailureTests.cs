using EnglishCoach.Application.Ports;
using EnglishCoach.Infrastructure.AI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EnglishCoach.UnitTests.Infrastructure;

public class ProviderFailureTests
{
    private readonly ProviderFailureHandler _handler;
    private readonly Mock<ILogger<ProviderFailureHandler>> _mockLogger;

    public ProviderFailureTests()
    {
        _mockLogger = new Mock<ILogger<ProviderFailureHandler>>();
        _handler = new ProviderFailureHandler(_mockLogger.Object);
    }

    [Fact]
    public void HandleTranscriptionFailure_ShouldBeRecoverable_ForTimeout()
    {
        // Arrange
        var result = TranscriptionResult.Failure("TIMEOUT", "Request timed out", ProviderKind.OpenAI);
        var attemptId = Guid.NewGuid();
        var correlationId = "ABC123";

        // Act
        var failure = _handler.HandleTranscriptionFailure(result, attemptId, correlationId);

        // Assert
        failure.IsRecoverable.Should().BeTrue();
        failure.ErrorCode.Should().Be("TIMEOUT");
        failure.DomainEvent.Should().BeOfType<SpeechTranscriptionFailed>();
    }

    [Fact]
    public void HandleTranscriptionFailure_ShouldBeRecoverable_ForRateLimited()
    {
        var result = TranscriptionResult.Failure("RATE_LIMITED", "Rate limit exceeded", ProviderKind.OpenAI);

        var failure = _handler.HandleTranscriptionFailure(result, Guid.NewGuid(), "XYZ789");

        failure.IsRecoverable.Should().BeTrue();
    }

    [Fact]
    public void HandleTranscriptionFailure_ShouldNotBeRecoverable_ForTranscriptionFailed()
    {
        var result = TranscriptionResult.Failure("TRANSCRIPTION_FAILED", "Audio unclear", ProviderKind.OpenAI);

        var failure = _handler.HandleTranscriptionFailure(result, Guid.NewGuid(), "DEF456");

        failure.IsRecoverable.Should().BeFalse();
    }

    [Fact]
    public void HandleFeedbackFailure_ShouldTrackDomainEvent()
    {
        var result = FeedbackResult.Failure("TIMEOUT", "Service timed out", ProviderKind.OpenAI);
        var attemptId = Guid.NewGuid();

        var failure = _handler.HandleFeedbackFailure(result, attemptId, "GHI789");

        failure.DomainEvent.Should().BeOfType<FeedbackGenerationFailed>();
        var domainEvent = (FeedbackGenerationFailed)failure.DomainEvent;
        domainEvent.AttemptId.Should().Be(attemptId);
        domainEvent.ErrorCode.Should().Be("TIMEOUT");
    }

    [Fact]
    public void HandleRoleplayFailure_ShouldBeRecoverable_ForServiceUnavailable()
    {
        var result = RoleplayResult.Failure("SERVICE_UNAVAILABLE", "Provider down", ProviderKind.OpenAI);

        var failure = _handler.HandleRoleplayFailure(result, Guid.NewGuid(), "JKL012");

        failure.IsRecoverable.Should().BeTrue();
    }

    [Fact]
    public void GenerateCorrelationId_ShouldReturnUniqueIds()
    {
        var id1 = ProviderFailureHandler.GenerateCorrelationId();
        var id2 = ProviderFailureHandler.GenerateCorrelationId();

        id1.Should().NotBe(id2);
        id1.Should().HaveLength(12);
        id2.Should().HaveLength(12);
    }

    [Fact]
    public void DomainEvents_ShouldContainCorrelationId()
    {
        var result = TranscriptionResult.Failure("TIMEOUT", "Timed out", ProviderKind.Fake);
        var correlationId = "TEST123";

        var failure = _handler.HandleTranscriptionFailure(result, Guid.NewGuid(), correlationId);

        var domainEvent = (SpeechTranscriptionFailed)failure.DomainEvent;
        domainEvent.CorrelationId.Should().Be(correlationId);
    }
}