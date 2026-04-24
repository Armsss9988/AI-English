using EnglishCoach.Domain.DailyMission;
using FluentAssertions;
using Xunit;

namespace EnglishCoach.UnitTests.DailyMission;

public class DailyMissionPolicyTests
{
    [Fact]
    public void Default_ShouldHaveCorrectValues()
    {
        // Act
        var policy = DailyMissionPolicies.Default;

        // Assert
        policy.DueReviewCount.Should().Be(5);
        policy.SpeakingDrillCount.Should().Be(1);
        policy.RoleplayScenarioCount.Should().Be(1);
        policy.RetryTaskCount.Should().Be(1);
        policy.CriticalErrorThreshold.Should().Be(1);
        policy.RecentDaysForCriticalError.Should().Be(7);
    }

    [Fact]
    public void NewPolicy_ShouldAllowCustomValues()
    {
        // Act
        var customPolicy = new DailyMissionPolicy(
            DueReviewCount: 10,
            SpeakingDrillCount: 2,
            RoleplayScenarioCount: 3,
            RetryTaskCount: 1,
            CriticalErrorThreshold: 5,
            RecentDaysForCriticalError: 30
        );

        // Assert
        customPolicy.DueReviewCount.Should().Be(10);
        customPolicy.SpeakingDrillCount.Should().Be(2);
        customPolicy.RoleplayScenarioCount.Should().Be(3);
        customPolicy.RetryTaskCount.Should().Be(1);
        customPolicy.CriticalErrorThreshold.Should().Be(5);
        customPolicy.RecentDaysForCriticalError.Should().Be(30);
    }
}

public class DailyMissionSelectionTests
{
    [Fact]
    public void TotalItems_ShouldSumAllSections()
    {
        // Arrange
        var selection = new DailyMissionSelection(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            new[] { CreateDueReview(), CreateDueReview() },
            new[] { CreateSpeakingTask() },
            new[] { CreateRoleplayTask() },
            Array.Empty<RetryTask>(),
            false
        );

        // Act
        var total = selection.TotalItems;

        // Assert
        total.Should().Be(4);
    }

    [Fact]
    public void IsComplete_ShouldBeTrue_WhenHasAnyItems()
    {
        // Arrange
        var selection = new DailyMissionSelection(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            new[] { CreateDueReview() },
            Array.Empty<SpeakingTask>(),
            Array.Empty<RoleplayTask>(),
            Array.Empty<RetryTask>(),
            false
        );

        // Assert
        selection.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void IsComplete_ShouldBeFalse_WhenAllSectionsEmpty()
    {
        // Arrange
        var selection = new DailyMissionSelection(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            Array.Empty<DueReviewItem>(),
            Array.Empty<SpeakingTask>(),
            Array.Empty<RoleplayTask>(),
            Array.Empty<RetryTask>(),
            false
        );

        // Assert
        selection.IsComplete.Should().BeFalse();
    }

    private static DueReviewItem CreateDueReview() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Test phrase", "greeting", DateTimeOffset.UtcNow);

    private static SpeakingTask CreateSpeakingTask() =>
        new(Guid.NewGuid(), "Test task", "pronunciation", Domain.LearningContent.ContentType.Phrase);

    private static RoleplayTask CreateRoleplayTask() =>
        new(Guid.NewGuid(), "Test scenario", "Practice", "Client", "standup");
}