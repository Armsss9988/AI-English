using EnglishCoach.Domain.Progress;

namespace EnglishCoach.Application.Progress;

public interface IProgressDataProvider
{
    Task<decimal> GetReviewCompletionRateAsync(Guid learnerId, CancellationToken ct = default);
    Task<decimal> GetPhraseMasteryAverageAsync(Guid learnerId, CancellationToken ct = default);
    Task<decimal> GetSpeakingTaskCompletionRateAsync(Guid learnerId, CancellationToken ct = default);
    Task<decimal> GetRoleplaySuccessRateAsync(Guid learnerId, CancellationToken ct = default);
    Task<decimal> GetCriticalErrorCountAsync(Guid learnerId, CancellationToken ct = default);
    Task<decimal> GetRetrySuccessRateAsync(Guid learnerId, CancellationToken ct = default);
}

public interface IReadinessSnapshotRepository
{
    Task AddAsync(ReadinessSnapshotEntity snapshot, CancellationToken ct = default);
    Task<ReadinessSnapshotEntity?> GetLatestAsync(Guid learnerId, CancellationToken ct = default);
}

public class RecalculateReadinessUseCase
{
    private readonly IProgressDataProvider _dataProvider;
    private readonly IReadinessSnapshotRepository _snapshotRepository;

    public RecalculateReadinessUseCase(
        IProgressDataProvider dataProvider,
        IReadinessSnapshotRepository snapshotRepository)
    {
        _dataProvider = dataProvider;
        _snapshotRepository = snapshotRepository;
    }

    public async Task<ReadinessSnapshotEntity> ExecuteAsync(Guid learnerId, CancellationToken ct = default)
    {
        var components = new ReadinessComponents(
            await _dataProvider.GetReviewCompletionRateAsync(learnerId, ct),
            await _dataProvider.GetPhraseMasteryAverageAsync(learnerId, ct),
            await _dataProvider.GetSpeakingTaskCompletionRateAsync(learnerId, ct),
            await _dataProvider.GetRoleplaySuccessRateAsync(learnerId, ct),
            await _dataProvider.GetCriticalErrorCountAsync(learnerId, ct),
            await _dataProvider.GetRetrySuccessRateAsync(learnerId, ct)
        );

        var score = ReadinessFormula.Calculate(learnerId, components);
        var snapshot = new ReadinessSnapshotEntity(learnerId, score);
        await _snapshotRepository.AddAsync(snapshot, ct);

        return snapshot;
    }
}
