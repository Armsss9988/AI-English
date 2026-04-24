using EnglishCoach.Contracts.Review;
using EnglishCoach.Domain.Review;
using EnglishCoach.SharedKernel.Time;

namespace EnglishCoach.Application.Review;

public sealed class CompleteReviewItemUseCase
{
    private readonly IReviewRepository _repository;
    private readonly IClock _clock;

    public CompleteReviewItemUseCase(IReviewRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<CompleteReviewItemResponse> ExecuteAsync(
        string userId,
        string reviewItemId,
        CompleteReviewItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(reviewItemId, userId, cancellationToken)
            ?? throw new InvalidOperationException("Review item not found.");

        var completedAtUtc = _clock.UtcNow;
        var previousState = item.MasteryState;
        var previousRepetitionCount = item.RepetitionCount;
        var quality = ReviewContractMapper.ParseQuality(request.Quality);
        var decision = ReviewSchedulingPolicy.Calculate(
            item.MasteryState,
            item.RepetitionCount,
            quality,
            completedAtUtc,
            item.DueAtUtc);

        var attempt = ReviewAttempt.Create(
            Guid.NewGuid().ToString("N"),
            item.Id,
            quality,
            previousState,
            decision.NextState,
            previousRepetitionCount,
            decision.NextRepetitionCount,
            completedAtUtc,
            decision.NextDueAtUtc);

        item.Complete(decision, completedAtUtc);

        await _repository.CompleteAsync(item, attempt, cancellationToken);

        return new CompleteReviewItemResponse(
            item.Id,
            ReviewContractMapper.ToContractValue(item.MasteryState),
            item.DueAtUtc.ToString("O"),
            item.RepetitionCount);
    }
}
