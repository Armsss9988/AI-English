using EnglishCoach.Domain.DailyMission;
using EnglishCoach.SharedKernel.Clock;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnglishCoach.UnitTests.DailyMission;

public class DailyMissionSelectorTests
{
    private readonly Mock<IDailyMissionDataProvider> _mockDataProvider;
    private readonly Mock<IClock> _mockClock;
    private readonly DailyMissionSelector _selector;
    private readonly Guid _learnerId = Guid.NewGuid();

    public DailyMissionSelectorTests()
    {
        _mockDataProvider = new Mock<IDailyMissionDataProvider>();
        _mockClock = new Mock<IClock>();
        _mockClock.Setup(c => c.Today).Returns(new DateOnly(2026, 4, 24));
        _selector = new DailyMissionSelector(_mockDataProvider.Object, _mockClock.Object);
    }

    [Fact]
    public async Task SelectAsync_ShouldReturnDefaultPolicyItems()
    {
        // Arrange
        _mockDataProvider.Setup(x => x.GetDueReviewsAsync(_learnerId, 5, default))
            .ReturnsAsync(CreateDueReviews(5));
        _mockDataProvider.Setup(x => x.GetSpeakingDrillsAsync(1, default))
            .ReturnsAsync(CreateSpeakingTasks(1));
        _mockDataProvider.Setup(x => x.GetRoleplayScenariosAsync(null, 1, default))
            .ReturnsAsync(CreateRoleplayTasks(1));
        _mockDataProvider.Setup(x => x.GetCriticalErrorCountAsync(_learnerId, 7, default))
            .ReturnsAsync(0);

        var policy = DailyMissionPolicies.Default;

        // Act
        var result = await _selector.SelectAsync(_learnerId, policy);

        // Assert
        result.LearnerId.Should().Be(_learnerId);
        result.MissionDate.Should().Be(new DateOnly(2026, 4, 24));
        result.DueReviews.Should().HaveCount(5);
        result.SpeakingDrills.Should().HaveCount(1);
        result.RoleplayScenarios.Should().HaveCount(1);
        result.HasRetryTask.Should().BeFalse();
    }

    [Fact]
    public async Task SelectAsync_ShouldIncludeRetryTask_WhenCriticalErrorsExist()
    {
        // Arrange
        var retryTasks = CreateRetryTasks(1);
        _mockDataProvider.Setup(x => x.GetDueReviewsAsync(_learnerId, 5, default))
            .ReturnsAsync(CreateDueReviews(5));
        _mockDataProvider.Setup(x => x.GetSpeakingDrillsAsync(1, default))
            .ReturnsAsync(CreateSpeakingTasks(1));
        _mockDataProvider.Setup(x => x.GetRoleplayScenariosAsync(null, 1, default))
            .ReturnsAsync(CreateRoleplayTasks(1));
        _mockDataProvider.Setup(x => x.GetCriticalErrorCountAsync(_learnerId, 7, default))
            .ReturnsAsync(2); // >= threshold of 1
        _mockDataProvider.Setup(x => x.GetRecentCriticalErrorsAsync(_learnerId, 1, default))
            .ReturnsAsync(retryTasks);

        var policy = DailyMissionPolicies.Default;

        // Act
        var result = await _selector.SelectAsync(_learnerId, policy);

        // Assert
        result.HasRetryTask.Should().BeTrue();
        result.RetryTasks.Should().HaveCount(1);
    }

    [Fact]
    public async Task SelectAsync_ShouldNotIncludeRetryTask_WhenNoCriticalErrors()
    {
        // Arrange
        _mockDataProvider.Setup(x => x.GetDueReviewsAsync(_learnerId, 5, default))
            .ReturnsAsync(CreateDueReviews(5));
        _mockDataProvider.Setup(x => x.GetSpeakingDrillsAsync(1, default))
            .ReturnsAsync(CreateSpeakingTasks(1));
        _mockDataProvider.Setup(x => x.GetRoleplayScenariosAsync(null, 1, default))
            .ReturnsAsync(CreateRoleplayTasks(1));
        _mockDataProvider.Setup(x => x.GetCriticalErrorCountAsync(_learnerId, 7, default))
            .ReturnsAsync(0); // No critical errors

        var policy = DailyMissionPolicies.Default;

        // Act
        var result = await _selector.SelectAsync(_learnerId, policy);

        // Assert
        result.HasRetryTask.Should().BeFalse();
        result.RetryTasks.Should().BeEmpty();
    }

    [Fact]
    public async Task SelectAsync_ShouldBeDeterministic_SameInputProducesSameOutput()
    {
        // Arrange
        _mockDataProvider.Setup(x => x.GetDueReviewsAsync(_learnerId, 5, default))
            .ReturnsAsync(CreateDueReviews(5));
        _mockDataProvider.Setup(x => x.GetSpeakingDrillsAsync(1, default))
            .ReturnsAsync(CreateSpeakingTasks(1));
        _mockDataProvider.Setup(x => x.GetRoleplayScenariosAsync(null, 1, default))
            .ReturnsAsync(CreateRoleplayTasks(1));
        _mockDataProvider.Setup(x => x.GetCriticalErrorCountAsync(_learnerId, 7, default))
            .ReturnsAsync(0);

        var policy = DailyMissionPolicies.Default;

        // Act
        var result1 = await _selector.SelectAsync(_learnerId, policy);
        var result2 = await _selector.SelectAsync(_learnerId, policy);

        // Assert
        result1.TotalItems.Should().Be(result2.TotalItems);
        result1.DueReviews.Count.Should().Be(result2.DueReviews.Count);
        result1.HasRetryTask.Should().Be(result2.HasRetryTask);
    }

    [Fact]
    public void SelectWithGracefulDegradation_ShouldReturnEmptyMission()
    {
        // Act
        var result = _selector.SelectWithGracefulDegradation(
            _learnerId,
            DailyMissionPolicies.Default,
            new FailingDataProvider());

        // Assert
        result.DueReviews.Should().BeEmpty();
        result.SpeakingDrills.Should().BeEmpty();
        result.RoleplayScenarios.Should().BeEmpty();
        result.HasRetryTask.Should().BeFalse();
        result.IsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task SelectAsync_WithCustomPolicy_ShouldUseCustomValues()
    {
        // Arrange
        var customPolicy = new DailyMissionPolicy(
            DueReviewCount: 10,
            SpeakingDrillCount: 2,
            RoleplayScenarioCount: 3,
            RetryTaskCount: 2,
            CriticalErrorThreshold: 2,
            RecentDaysForCriticalError: 14
        );

        _mockDataProvider.Setup(x => x.GetDueReviewsAsync(_learnerId, 10, default))
            .ReturnsAsync(CreateDueReviews(10));
        _mockDataProvider.Setup(x => x.GetSpeakingDrillsAsync(2, default))
            .ReturnsAsync(CreateSpeakingTasks(2));
        _mockDataProvider.Setup(x => x.GetRoleplayScenariosAsync(null, 3, default))
            .ReturnsAsync(CreateRoleplayTasks(3));
        _mockDataProvider.Setup(x => x.GetCriticalErrorCountAsync(_learnerId, 14, default))
            .ReturnsAsync(3);
        _mockDataProvider.Setup(x => x.GetRecentCriticalErrorsAsync(_learnerId, 2, default))
            .ReturnsAsync(CreateRetryTasks(2));

        // Act
        var result = await _selector.SelectAsync(_learnerId, customPolicy);

        // Assert
        result.DueReviews.Should().HaveCount(10);
        result.SpeakingDrills.Should().HaveCount(2);
        result.RoleplayScenarios.Should().HaveCount(3);
        result.HasRetryTask.Should().BeTrue();
        result.RetryTasks.Should().HaveCount(2);
    }

    private static IReadOnlyList<DueReviewItem> CreateDueReviews(int count) =>
        Enumerable.Range(0, count).Select(i => new DueReviewItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            $"Phrase {i}",
            "greeting",
            DateTimeOffset.UtcNow
        )).ToList();

    private static IReadOnlyList<SpeakingTask> CreateSpeakingTasks(int count) =>
        Enumerable.Range(0, count).Select(i => new SpeakingTask(
            Guid.NewGuid(),
            $"Speaking Task {i}",
            "pronunciation",
            Domain.LearningContent.ContentType.Phrase
        )).ToList();

    private static IReadOnlyList<RoleplayTask> CreateRoleplayTasks(int count) =>
        Enumerable.Range(0, count).Select(i => new RoleplayTask(
            Guid.NewGuid(),
            $"Scenario {i}",
            "Practice daily standup",
            "Client PM",
            "standup"
        )).ToList();

    private static IReadOnlyList<RetryTask> CreateRetryTasks(int count) =>
        Enumerable.Range(0, count).Select(i => new RetryTask(
            Guid.NewGuid(),
            $"Error pattern {i}",
            $"Corrected form {i}",
            "grammar"
        )).ToList();

    private class FailingDataProvider : IDailyMissionDataProvider
    {
        public Task<IReadOnlyList<DueReviewItem>> GetDueReviewsAsync(Guid learnerId, int limit, CancellationToken ct = default)
            => throw new Exception("Data provider unavailable");

        public Task<IReadOnlyList<SpeakingTask>> GetSpeakingDrillsAsync(int limit, CancellationToken ct = default)
            => throw new Exception("Data provider unavailable");

        public Task<IReadOnlyList<RoleplayTask>> GetRoleplayScenariosAsync(string? excludeGroup, int limit, CancellationToken ct = default)
            => throw new Exception("Data provider unavailable");

        public Task<int> GetCriticalErrorCountAsync(Guid learnerId, int recentDays, CancellationToken ct = default)
            => throw new Exception("Data provider unavailable");

        public Task<IReadOnlyList<RetryTask>> GetRecentCriticalErrorsAsync(Guid learnerId, int limit, CancellationToken ct = default)
            => throw new Exception("Data provider unavailable");
    }
}