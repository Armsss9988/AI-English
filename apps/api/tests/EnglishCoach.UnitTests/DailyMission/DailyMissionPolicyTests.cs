using EnglishCoach.Domain.DailyMission;

namespace EnglishCoach.UnitTests.DailyMission;

public class DailyMissionPolicyTests
{
    [Fact]
    public void Default_HasCorrectValues()
    {
        var policy = DailyMissionPolicies.Default;

        Assert.Equal(5, policy.DueReviewCount);
        Assert.Equal(1, policy.SpeakingDrillCount);
        Assert.Equal(1, policy.RoleplayScenarioCount);
        Assert.Equal(1, policy.RetryTaskCount);
        Assert.Equal(1, policy.CriticalErrorThreshold);
        Assert.Equal(7, policy.RecentDaysForCriticalError);
    }

    [Fact]
    public void NewPolicy_AllowsCustomValues()
    {
        var custom = new DailyMissionPolicy(
            DueReviewCount: 10,
            SpeakingDrillCount: 2,
            RoleplayScenarioCount: 3,
            RetryTaskCount: 1,
            CriticalErrorThreshold: 5,
            RecentDaysForCriticalError: 30);

        Assert.Equal(10, custom.DueReviewCount);
        Assert.Equal(2, custom.SpeakingDrillCount);
        Assert.Equal(3, custom.RoleplayScenarioCount);
        Assert.Equal(5, custom.CriticalErrorThreshold);
        Assert.Equal(30, custom.RecentDaysForCriticalError);
    }
}

public class DailyMissionSelectionTests
{
    [Fact]
    public void TotalItems_SumsAllSections()
    {
        var selection = new DailyMissionSelection(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            new[] { CreateReview(), CreateReview() },
            new[] { CreateSpeaking() },
            new[] { CreateRoleplay() },
            Array.Empty<RetryTask>(),
            false);

        Assert.Equal(4, selection.TotalItems);
    }

    [Fact]
    public void IsComplete_True_WhenHasItems()
    {
        var selection = new DailyMissionSelection(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            new[] { CreateReview() },
            Array.Empty<SpeakingTask>(),
            Array.Empty<RoleplayTask>(),
            Array.Empty<RetryTask>(),
            false);

        Assert.True(selection.IsComplete);
    }

    [Fact]
    public void IsComplete_False_WhenAllEmpty()
    {
        var selection = new DailyMissionSelection(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            Array.Empty<DueReviewItem>(),
            Array.Empty<SpeakingTask>(),
            Array.Empty<RoleplayTask>(),
            Array.Empty<RetryTask>(),
            false);

        Assert.False(selection.IsComplete);
    }

    private static DueReviewItem CreateReview() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Phrase", "greeting", DateTimeOffset.UtcNow);

    private static SpeakingTask CreateSpeaking() =>
        new(Guid.NewGuid(), "Task", "pronunciation", default);

    private static RoleplayTask CreateRoleplay() =>
        new(Guid.NewGuid(), "Scenario", "Practice", "Client", "standup");
}
