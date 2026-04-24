namespace EnglishCoach.Domain.Review;

public sealed class ReviewItem
{
    private ReviewItem()
    {
        Id = string.Empty;
        UserId = string.Empty;
        ItemId = string.Empty;
        DisplayText = string.Empty;
    }

    private ReviewItem(
        string id,
        string userId,
        string itemId,
        ReviewTrack reviewTrack,
        string displayText,
        string? displaySubtitle,
        DateTimeOffset dueAtUtc,
        DateTimeOffset createdAtUtc)
    {
        Id = Require(id, nameof(id), 64);
        UserId = Require(userId, nameof(userId), 128);
        ItemId = Require(itemId, nameof(itemId), 128);
        DisplayText = Require(displayText, nameof(displayText), 240);
        DisplaySubtitle = displaySubtitle?.Trim();
        ReviewTrack = reviewTrack;
        MasteryState = ReviewMasteryState.New;
        RepetitionCount = 0;
        DueAtUtc = dueAtUtc;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Id { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public string ItemId { get; private set; } = string.Empty;
    public ReviewTrack ReviewTrack { get; private set; }
    public string DisplayText { get; private set; } = string.Empty;
    public string? DisplaySubtitle { get; private set; }
    public ReviewMasteryState MasteryState { get; private set; }
    public int RepetitionCount { get; private set; }
    public DateTimeOffset DueAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? LastCompletedAtUtc { get; private set; }

    public static ReviewItem Create(
        string id,
        string userId,
        string itemId,
        ReviewTrack reviewTrack,
        string displayText,
        string? displaySubtitle,
        DateTimeOffset dueAtUtc,
        DateTimeOffset createdAtUtc)
    {
        return new ReviewItem(id, userId, itemId, reviewTrack, displayText, displaySubtitle, dueAtUtc, createdAtUtc);
    }

    public bool IsDue(DateTimeOffset nowUtc) => DueAtUtc <= nowUtc;

    public void UpdateDisplay(string displayText, string? displaySubtitle, DateTimeOffset updatedAtUtc)
    {
        DisplayText = Require(displayText, nameof(displayText), 240);
        DisplaySubtitle = displaySubtitle?.Trim();
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Complete(ReviewScheduleDecision decision, DateTimeOffset completedAtUtc)
    {
        ReviewMasteryStateMachine.AssertCanTransition(MasteryState, decision.NextState);
        MasteryState = decision.NextState;
        RepetitionCount = decision.NextRepetitionCount;
        DueAtUtc = decision.NextDueAtUtc;
        LastCompletedAtUtc = completedAtUtc;
        UpdatedAtUtc = completedAtUtc;
    }

    private static string Require(string value, string paramName, int maxLength)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Value must be {maxLength} characters or fewer.");
        }

        return trimmed;
    }
}
