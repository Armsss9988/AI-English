using EnglishCoach.Domain.Progress;

namespace EnglishCoach.Application.Progress;

public interface IProgressDataProvider
{
    Task<decimal> GetReviewCompletionRateAsync(string learnerId, CancellationToken ct = default);
    Task<decimal> GetPhraseMasteryAverageAsync(string learnerId, CancellationToken ct = default);
    Task<decimal> GetSpeakingTaskCompletionRateAsync(string learnerId, CancellationToken ct = default);
    Task<decimal> GetRoleplaySuccessRateAsync(string learnerId, CancellationToken ct = default);
    Task<decimal> GetCriticalErrorCountAsync(string learnerId, CancellationToken ct = default);
    Task<decimal> GetRetrySuccessRateAsync(string learnerId, CancellationToken ct = default);
}

public interface IReadinessSnapshotRepository
{
    Task AddAsync(ReadinessSnapshotEntity snapshot, CancellationToken ct = default);
    Task<ReadinessSnapshotEntity?> GetLatestAsync(string learnerId, CancellationToken ct = default);
}

public sealed class RecalculateReadinessUseCase
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

    public async Task<ReadinessScore> ExecuteAsync(string learnerId, CancellationToken ct = default)
    {
        var learnGuid = Guid.TryParse(learnerId, out var g) ? g : Guid.Empty;

        var components = new ReadinessComponents(
            await _dataProvider.GetReviewCompletionRateAsync(learnerId, ct),
            await _dataProvider.GetPhraseMasteryAverageAsync(learnerId, ct),
            await _dataProvider.GetSpeakingTaskCompletionRateAsync(learnerId, ct),
            await _dataProvider.GetRoleplaySuccessRateAsync(learnerId, ct),
            await _dataProvider.GetCriticalErrorCountAsync(learnerId, ct),
            await _dataProvider.GetRetrySuccessRateAsync(learnerId, ct)
        );

        // Calculate score (pure function)
        var score = ReadinessFormula.Calculate(learnGuid, components);

        // Create immutable snapshot
        var snapshot = new ReadinessSnapshotEntity(learnGuid, score);

        // Persist (append only, never update)
        await _snapshotRepository.AddAsync(snapshot, ct);

        return score;
    }
}
