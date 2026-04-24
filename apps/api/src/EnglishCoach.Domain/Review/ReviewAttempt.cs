namespace EnglishCoach.Domain.Review;

public sealed class ReviewAttempt
{
    private ReviewAttempt()
    {
        Id = string.Empty;
        ReviewItemId = string.Empty;
    }

    private ReviewAttempt(
        string id,
        string reviewItemId,
        ReviewQuality quality,
        ReviewMasteryState previousState,
        ReviewMasteryState nextState,
        int previousRepetitionCount,
        int nextRepetitionCount,
        DateTimeOffset completedAtUtc,
        DateTimeOffset nextDueAtUtc)
    {
        Id = id;
        ReviewItemId = reviewItemId;
        Quality = quality;
        PreviousState = previousState;
        NextState = nextState;
        PreviousRepetitionCount = previousRepetitionCount;
        NextRepetitionCount = nextRepetitionCount;
        CompletedAtUtc = completedAtUtc;
        NextDueAtUtc = nextDueAtUtc;
    }

    public string Id { get; private set; } = string.Empty;
    public string ReviewItemId { get; private set; } = string.Empty;
    public ReviewQuality Quality { get; private set; }
    public ReviewMasteryState PreviousState { get; private set; }
    public ReviewMasteryState NextState { get; private set; }
    public int PreviousRepetitionCount { get; private set; }
    public int NextRepetitionCount { get; private set; }
    public DateTimeOffset CompletedAtUtc { get; private set; }
    public DateTimeOffset NextDueAtUtc { get; private set; }

    public static ReviewAttempt Create(
        string id,
        string reviewItemId,
        ReviewQuality quality,
        ReviewMasteryState previousState,
        ReviewMasteryState nextState,
        int previousRepetitionCount,
        int nextRepetitionCount,
        DateTimeOffset completedAtUtc,
        DateTimeOffset nextDueAtUtc)
    {
        return new ReviewAttempt(
            id,
            reviewItemId,
            quality,
            previousState,
            nextState,
            previousRepetitionCount,
            nextRepetitionCount,
            completedAtUtc,
            nextDueAtUtc);
    }
}
