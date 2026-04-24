using EnglishCoach.Domain.Progress;
using EnglishCoach.SharedKernel.Ids;

namespace EnglishCoach.Application.UseCases;

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
        // Fetch all components
        var components = new ReadinessComponents(
            await _dataProvider.GetReviewCompletionRateAsync(learnerId, ct),
            await _dataProvider.GetPhraseMasteryAverageAsync(learnerId, ct),
            await _dataProvider.GetSpeakingTaskCompletionRateAsync(learnerId, ct),
            await _dataProvider.GetRoleplaySuccessRateAsync(learnerId, ct),
            await _dataProvider.GetCriticalErrorCountAsync(learnerId, ct),
            await _dataProvider.GetRetrySuccessRateAsync(learnerId, ct)
        );

        // Calculate score (pure function)
        var score = ReadinessFormula.Calculate(learnerId, components);

        // Create immutable snapshot
        var snapshot = new ReadinessSnapshotEntity(learnerId, score);

        // Persist (append only, never update)
        await _snapshotRepository.AddAsync(snapshot, ct);

        return snapshot;
    }
}