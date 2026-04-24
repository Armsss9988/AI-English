using EnglishCoach.Contracts.Review;
using EnglishCoach.Domain.Review;
using EnglishCoach.SharedKernel.Time;

namespace EnglishCoach.Application.Review;

public sealed class EnsureReviewItemExistsUseCase
{
    private readonly IReviewRepository _repository;
    private readonly IClock _clock;

    public EnsureReviewItemExistsUseCase(IReviewRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<EnsureReviewItemResponse> ExecuteAsync(EnsureReviewItemRequest request, CancellationToken cancellationToken)
    {
        var reviewTrack = ReviewContractMapper.ParseTrack(request.ReviewTrack);
        var existing = await _repository.GetByCompositeKeyAsync(request.UserId, request.ItemId, reviewTrack, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateDisplay(request.DisplayText, request.DisplaySubtitle, _clock.UtcNow);
            await _repository.UpdateAsync(existing, cancellationToken);
            return new EnsureReviewItemResponse(existing.Id);
        }

        var created = ReviewItem.Create(
            Guid.NewGuid().ToString("N"),
            request.UserId,
            request.ItemId,
            reviewTrack,
            request.DisplayText,
            request.DisplaySubtitle,
            _clock.UtcNow,
            _clock.UtcNow);

        try
        {
            await _repository.CreateAsync(created, cancellationToken);
        }
        catch (DuplicateReviewItemException)
        {
            existing = await _repository.GetByCompositeKeyAsync(request.UserId, request.ItemId, reviewTrack, cancellationToken);

            if (existing is not null)
            {
                return new EnsureReviewItemResponse(existing.Id);
            }

            throw;
        }

        return new EnsureReviewItemResponse(created.Id);
    }
}
