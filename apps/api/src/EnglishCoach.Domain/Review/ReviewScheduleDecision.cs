namespace EnglishCoach.Domain.Review;

public sealed record ReviewScheduleDecision(
    ReviewMasteryState NextState,
    DateTimeOffset NextDueAtUtc,
    int NextRepetitionCount);
