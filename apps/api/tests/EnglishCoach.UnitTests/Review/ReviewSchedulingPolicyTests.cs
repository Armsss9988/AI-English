using EnglishCoach.Domain.Review;

namespace EnglishCoach.UnitTests.Review;

public sealed class ReviewSchedulingPolicyTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 24, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FirstReview_Good_Advances_To_Learning()
    {
        var decision = ReviewSchedulingPolicy.Calculate(
            ReviewMasteryState.New,
            repetitionCount: 0,
            ReviewQuality.Good,
            Now,
            dueAtUtc: Now);

        Assert.Equal(ReviewMasteryState.Learning, decision.NextState);
        Assert.Equal(1, decision.NextRepetitionCount);
        Assert.Equal(Now.AddHours(12), decision.NextDueAtUtc);
    }

    [Fact]
    public void FailedReview_Regresses_And_Shortens_Interval()
    {
        var decision = ReviewSchedulingPolicy.Calculate(
            ReviewMasteryState.Review,
            repetitionCount: 3,
            ReviewQuality.Again,
            Now,
            dueAtUtc: Now.AddDays(-2));

        Assert.Equal(ReviewMasteryState.Weak, decision.NextState);
        Assert.Equal(0, decision.NextRepetitionCount);
        Assert.Equal(Now.AddMinutes(10), decision.NextDueAtUtc);
    }

    [Fact]
    public void GoodReview_OnTime_Advances_With_Daily_Interval()
    {
        var decision = ReviewSchedulingPolicy.Calculate(
            ReviewMasteryState.Learning,
            repetitionCount: 1,
            ReviewQuality.Good,
            Now,
            dueAtUtc: Now);

        Assert.Equal(ReviewMasteryState.Weak, decision.NextState);
        Assert.Equal(2, decision.NextRepetitionCount);
        Assert.Equal(Now.AddDays(2), decision.NextDueAtUtc);
    }

    [Fact]
    public void EasyReview_Advances_Faster()
    {
        var decision = ReviewSchedulingPolicy.Calculate(
            ReviewMasteryState.Weak,
            repetitionCount: 2,
            ReviewQuality.Easy,
            Now,
            dueAtUtc: Now);

        Assert.Equal(ReviewMasteryState.Strong, decision.NextState);
        Assert.Equal(4, decision.NextRepetitionCount);
        Assert.Equal(Now.AddDays(10), decision.NextDueAtUtc);
    }

    [Fact]
    public void OverdueReview_Good_Adds_Extra_Recovery_Time()
    {
        var decision = ReviewSchedulingPolicy.Calculate(
            ReviewMasteryState.Review,
            repetitionCount: 3,
            ReviewQuality.Good,
            Now,
            dueAtUtc: Now.AddDays(-4));

        Assert.Equal(ReviewMasteryState.Strong, decision.NextState);
        Assert.Equal(4, decision.NextRepetitionCount);
        Assert.Equal(Now.AddDays(8), decision.NextDueAtUtc);
    }
}
