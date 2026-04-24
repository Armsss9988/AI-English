namespace EnglishCoach.Domain.Review;

public static class ReviewSchedulingPolicy
{
    public static ReviewScheduleDecision Calculate(
        ReviewMasteryState previousState,
        int repetitionCount,
        ReviewQuality quality,
        DateTimeOffset nowUtc,
        DateTimeOffset dueAtUtc)
    {
        var overdue = dueAtUtc < nowUtc;

        return quality switch
        {
            ReviewQuality.Again => new ReviewScheduleDecision(
                ReviewMasteryStateMachine.Regress(previousState),
                nowUtc.AddMinutes(10),
                0),
            ReviewQuality.Hard => new ReviewScheduleDecision(
                previousState == ReviewMasteryState.New
                    ? ReviewMasteryState.Learning
                    : previousState,
                nowUtc.AddDays(Math.Max(1, repetitionCount)),
                Math.Max(1, repetitionCount)),
            ReviewQuality.Good => new ReviewScheduleDecision(
                ReviewMasteryStateMachine.Advance(previousState),
                GetGoodDueAt(previousState, repetitionCount, overdue, nowUtc),
                repetitionCount + 1),
            ReviewQuality.Easy => new ReviewScheduleDecision(
                ReviewMasteryStateMachine.Advance(ReviewMasteryStateMachine.Advance(previousState)),
                nowUtc.AddDays(GetEasyDays(repetitionCount)),
                repetitionCount + 2),
            _ => throw new ArgumentOutOfRangeException(nameof(quality))
        };
    }

    private static DateTimeOffset GetGoodDueAt(
        ReviewMasteryState previousState,
        int repetitionCount,
        bool overdue,
        DateTimeOffset nowUtc)
    {
        if (repetitionCount == 0)
        {
            return nowUtc.AddHours(12);
        }

        var baseDays = previousState == ReviewMasteryState.Learning
            ? 2
            : Math.Max(3, repetitionCount * 2);

        if (overdue)
        {
            baseDays += 2;
        }

        return nowUtc.AddDays(baseDays);
    }

    private static int GetEasyDays(int repetitionCount)
    {
        return repetitionCount switch
        {
            <= 1 => 5,
            2 => 10,
            _ => Math.Max(14, repetitionCount * 4)
        };
    }
}
