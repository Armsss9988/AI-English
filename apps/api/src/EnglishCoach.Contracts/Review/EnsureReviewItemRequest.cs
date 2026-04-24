namespace EnglishCoach.Contracts.Review;

public sealed record EnsureReviewItemRequest(
    string UserId,
    string ItemId,
    string ReviewTrack,
    string DisplayText,
    string? DisplaySubtitle);
