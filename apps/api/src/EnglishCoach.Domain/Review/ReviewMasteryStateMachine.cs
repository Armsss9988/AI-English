namespace EnglishCoach.Domain.Review;

public static class ReviewMasteryStateMachine
{
    public static ReviewMasteryState Advance(ReviewMasteryState current)
    {
        return current switch
        {
            ReviewMasteryState.New => ReviewMasteryState.Learning,
            ReviewMasteryState.Learning => ReviewMasteryState.Weak,
            ReviewMasteryState.Weak => ReviewMasteryState.Review,
            ReviewMasteryState.Review => ReviewMasteryState.Strong,
            ReviewMasteryState.Strong => ReviewMasteryState.ClientReady,
            ReviewMasteryState.ClientReady => ReviewMasteryState.ClientReady,
            _ => throw new ArgumentOutOfRangeException(nameof(current))
        };
    }

    public static ReviewMasteryState Regress(ReviewMasteryState current)
    {
        return current switch
        {
            ReviewMasteryState.New => ReviewMasteryState.New,
            ReviewMasteryState.Learning => ReviewMasteryState.New,
            ReviewMasteryState.Weak => ReviewMasteryState.Learning,
            ReviewMasteryState.Review => ReviewMasteryState.Weak,
            ReviewMasteryState.Strong => ReviewMasteryState.Review,
            ReviewMasteryState.ClientReady => ReviewMasteryState.Strong,
            _ => throw new ArgumentOutOfRangeException(nameof(current))
        };
    }

    public static void AssertCanTransition(ReviewMasteryState current, ReviewMasteryState next)
    {
        if (next == current || next == Advance(current) || next == Regress(current))
        {
            return;
        }

        throw new InvalidOperationException($"Invalid review mastery transition from {current} to {next}.");
    }
}
