using EnglishCoach.Domain.Review;

namespace EnglishCoach.UnitTests.Review;

public sealed class ReviewMasteryStateMachineTests
{
    [Theory]
    [InlineData(ReviewMasteryState.New, ReviewMasteryState.Learning)]
    [InlineData(ReviewMasteryState.Learning, ReviewMasteryState.Weak)]
    [InlineData(ReviewMasteryState.Weak, ReviewMasteryState.Review)]
    [InlineData(ReviewMasteryState.Review, ReviewMasteryState.Strong)]
    [InlineData(ReviewMasteryState.Strong, ReviewMasteryState.ClientReady)]
    public void CanAdvance_To_Next_State(ReviewMasteryState current, ReviewMasteryState expected)
    {
        var next = ReviewMasteryStateMachine.Advance(current);

        Assert.Equal(expected, next);
    }

    [Theory]
    [InlineData(ReviewMasteryState.Learning, ReviewMasteryState.New)]
    [InlineData(ReviewMasteryState.Weak, ReviewMasteryState.Learning)]
    [InlineData(ReviewMasteryState.Review, ReviewMasteryState.Weak)]
    [InlineData(ReviewMasteryState.Strong, ReviewMasteryState.Review)]
    [InlineData(ReviewMasteryState.ClientReady, ReviewMasteryState.Strong)]
    public void CanRegress_To_Previous_State(ReviewMasteryState current, ReviewMasteryState expected)
    {
        var next = ReviewMasteryStateMachine.Regress(current);

        Assert.Equal(expected, next);
    }

    [Fact]
    public void Rejects_Invalid_Jump()
    {
        var action = () => ReviewMasteryStateMachine.AssertCanTransition(
            ReviewMasteryState.New,
            ReviewMasteryState.Review);

        Assert.Throws<InvalidOperationException>(action);
    }
}
