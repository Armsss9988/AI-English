using System.ComponentModel.DataAnnotations;

namespace EnglishCoach.Contracts.Review;

public sealed record CompleteReviewItemRequest(
    [property: Required, RegularExpression("again|hard|good|easy")]
    string Quality);
