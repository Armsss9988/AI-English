using EnglishCoach.Application.UseCases;
using EnglishCoach.Domain.Progress;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnglishCoach.UnitTests.Progress;

public class RecalculateReadinessTests
{
    private readonly Mock<IProgressDataProvider> _mockDataProvider;
    private readonly Mock<IReadinessSnapshotRepository> _mockRepository;
    private readonly RecalculateReadinessUseCase _useCase;
    private readonly Guid _learnerId = Guid.NewGuid();

    public RecalculateReadinessTests()
    {
        _mockDataProvider = new Mock<IProgressDataProvider>();
        _mockRepository = new Mock<IReadinessSnapshotRepository>();
        _useCase = new RecalculateReadinessUseCase(_mockDataProvider.Object, _mockRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateNewSnapshot_NotEditOld()
    {
        // Arrange
        SetupMockDataProviders();
        ReadinessSnapshotEntity? capturedSnapshot = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<ReadinessSnapshotEntity>(), default))
            .Callback<ReadinessSnapshotEntity, CancellationToken>((s, _) => capturedSnapshot = s);

        // Act
        var snapshot = await _useCase.ExecuteAsync(_learnerId);

        // Assert
        capturedSnapshot.Should().NotBeNull();
        capturedSnapshot!.LearnerId.Should().Be(_learnerId);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<ReadinessSnapshotEntity>(), default), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRecordFormulaVersion()
    {
        // Arrange
        SetupMockDataProviders();

        // Act
        var snapshot = await _useCase.ExecuteAsync(_learnerId);

        // Assert
        snapshot.FormulaVersion.Should().Be(ReadinessFormulaVersion);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateScore()
    {
        // Arrange
        SetupMockDataProviders();

        // Act
        var snapshot = await _useCase.ExecuteAsync(_learnerId);

        // Assert
        snapshot.Score.Should().BeGreaterOrEqualTo(0);
        snapshot.Score.Should().BeLessOrEqualTo(100);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFetchAllDataProviders()
    {
        // Arrange
        SetupMockDataProviders();

        // Act
        await _useCase.ExecuteAsync(_learnerId);

        // Assert
        _mockDataProvider.Verify(p => p.GetReviewCompletionRateAsync(_learnerId, default), Times.Once);
        _mockDataProvider.Verify(p => p.GetPhraseMasteryAverageAsync(_learnerId, default), Times.Once);
        _mockDataProvider.Verify(p => p.GetSpeakingTaskCompletionRateAsync(_learnerId, default), Times.Once);
        _mockDataProvider.Verify(p => p.GetRoleplaySuccessRateAsync(_learnerId, default), Times.Once);
        _mockDataProvider.Verify(p => p.GetCriticalErrorCountAsync(_learnerId, default), Times.Once);
        _mockDataProvider.Verify(p => p.GetRetrySuccessRateAsync(_learnerId, default), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CalledTwice_ShouldCreateTwoSnapshots()
    {
        // Arrange
        SetupMockDataProviders();

        // Act
        await _useCase.ExecuteAsync(_learnerId);
        await _useCase.ExecuteAsync(_learnerId);

        // Assert
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<ReadinessSnapshotEntity>(), default), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStoreComponentsJson()
    {
        // Arrange
        SetupMockDataProviders();

        // Act
        var snapshot = await _useCase.ExecuteAsync(_learnerId);

        // Assert
        snapshot.ComponentsJson.Should().NotBeNullOrEmpty();
        var components = snapshot.GetComponents();
        components.Should().NotBeEmpty();
    }

    private void SetupMockDataProviders()
    {
        _mockDataProvider.Setup(p => p.GetReviewCompletionRateAsync(_learnerId, default))
            .ReturnsAsync(0.8m);
        _mockDataProvider.Setup(p => p.GetPhraseMasteryAverageAsync(_learnerId, default))
            .ReturnsAsync(0.7m);
        _mockDataProvider.Setup(p => p.GetSpeakingTaskCompletionRateAsync(_learnerId, default))
            .ReturnsAsync(0.9m);
        _mockDataProvider.Setup(p => p.GetRoleplaySuccessRateAsync(_learnerId, default))
            .ReturnsAsync(0.6m);
        _mockDataProvider.Setup(p => p.GetCriticalErrorCountAsync(_learnerId, default))
            .ReturnsAsync(2m);
        _mockDataProvider.Setup(p => p.GetRetrySuccessRateAsync(_learnerId, default))
            .ReturnsAsync(0.5m);
    }
}