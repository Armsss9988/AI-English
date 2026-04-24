namespace EnglishCoach.Contracts.Review;

public sealed record GetDueReviewItemsResponse(IReadOnlyList<DueReviewItemResponse> Items);
