namespace EnglishCoach.Contracts.Review;

public sealed record DueReviewItemResponse(
    string ReviewItemId,
    string ItemId,
    string ReviewTrack,
    string DisplayText,
    string? DisplaySubtitle,
    string MasteryState,
    int RepetitionCount,
    string DueAtUtc);
