using EnglishCoach.Contracts.Review;
using EnglishCoach.Domain.Review;

namespace EnglishCoach.Application.Review;

internal static class ReviewContractMapper
{
    public static DueReviewItemResponse ToResponse(DueReviewItemReadModel item)
    {
        return new DueReviewItemResponse(
            item.ReviewItemId,
            item.ItemId,
            item.ReviewTrack,
            item.DisplayText,
            item.DisplaySubtitle,
            item.MasteryState,
            item.RepetitionCount,
            item.DueAtUtc.ToString("O"));
    }

    public static ReviewTrack ParseTrack(string reviewTrack)
    {
        return Enum.Parse<ReviewTrack>(reviewTrack, ignoreCase: true);
    }

    public static ReviewQuality ParseQuality(string quality)
    {
        return Enum.Parse<ReviewQuality>(quality, ignoreCase: true);
    }

    public static string ToContractValue(ReviewMasteryState state)
    {
        return state.ToString().ToLowerInvariant().Replace("clientready", "client_ready");
    }
}
