namespace EnglishCoach.Contracts.Review;

public sealed record CompleteReviewItemResponse(
    string ReviewItemId,
    string NextMasteryState,
    string NextDueAtUtc,
    int RepetitionCount);
