using EnglishCoach.Contracts.Review;
using EnglishCoach.SharedKernel.Time;

namespace EnglishCoach.Application.Review;

public sealed class GetDueReviewItemsUseCase
{
    private readonly IReviewRepository _repository;
    private readonly IClock _clock;

    public GetDueReviewItemsUseCase(IReviewRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<GetDueReviewItemsResponse> ExecuteAsync(string userId, CancellationToken cancellationToken)
    {
        var items = await _repository.GetDueItemsAsync(userId, _clock.UtcNow, cancellationToken);
        return new GetDueReviewItemsResponse(items.Select(ReviewContractMapper.ToResponse).ToArray());
    }
}
