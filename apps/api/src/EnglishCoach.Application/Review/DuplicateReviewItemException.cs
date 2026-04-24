namespace EnglishCoach.Application.Review;

public sealed class DuplicateReviewItemException : Exception
{
    public DuplicateReviewItemException()
        : base("A review item with the same user, item, and track already exists.")
    {
    }
}
