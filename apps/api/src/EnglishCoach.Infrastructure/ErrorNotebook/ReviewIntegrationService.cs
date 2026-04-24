using EnglishCoach.Application.ErrorNotebook;
using EnglishCoach.Application.Review;

namespace EnglishCoach.Infrastructure.ErrorNotebook;

public sealed class ReviewIntegrationService : IReviewIntegrationService
{
    private readonly EnsureReviewItemExistsUseCase _useCase;

    public ReviewIntegrationService(EnsureReviewItemExistsUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task EnsureReviewItemExistsAsync(string learnerId, string patternKey, CancellationToken ct = default)
    {
        var request = new EnglishCoach.Contracts.Review.EnsureReviewItemRequest(
            learnerId,
            patternKey,
            "Error", // Maps to Domain.Review.ReviewTrack.Error string value in ContractMapper
            $"Error pattern: {patternKey}",
            null
        );
        await _useCase.ExecuteAsync(request, ct);
    }
}
