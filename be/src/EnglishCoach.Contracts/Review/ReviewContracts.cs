namespace EnglishCoach.Contracts.Review;

public record GetDueReviewsRequest(
    Guid LearnerId,
    int Limit = 20
);

public record GetDueReviewsResponse(
    IReadOnlyList<DueReviewItem> Items,
    int TotalDue
);

public record DueReviewItem(
    Guid ReviewItemId,
    Guid PhraseId,
    string PhraseText,
    string Category,
    DateTimeOffset DueAt,
    int TimesReviewed,
    decimal MasteryLevel
);

public record CompleteReviewRequest(
    Guid ReviewItemId,
    Guid LearnerId,
    string Outcome,
    DateTimeOffset CompletedAt
);

public record CompleteReviewResponse(
    Guid ReviewItemId,
    string NewState,
    DateTimeOffset NextDueAt
);

public enum ReviewOutcome
{
    Again,
    Hard,
    Good,
    Easy
}