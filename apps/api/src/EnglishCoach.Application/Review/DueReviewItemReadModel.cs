namespace EnglishCoach.Application.Review;

public sealed record DueReviewItemReadModel(
    string ReviewItemId,
    string ItemId,
    string ReviewTrack,
    string DisplayText,
    string? DisplaySubtitle,
    string MasteryState,
    int RepetitionCount,
    DateTimeOffset DueAtUtc);
