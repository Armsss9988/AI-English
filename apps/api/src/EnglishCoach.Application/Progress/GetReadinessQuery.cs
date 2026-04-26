using EnglishCoach.Contracts.Progress;
using EnglishCoach.Domain.Progress;

namespace EnglishCoach.Application.Progress;

public class GetReadinessQuery
{
    private readonly IReadinessSnapshotRepository _snapshotRepository;
    private readonly RecalculateReadinessUseCase _recalculateUseCase;

    public GetReadinessQuery(
        IReadinessSnapshotRepository snapshotRepository,
        RecalculateReadinessUseCase recalculateUseCase)
    {
        _snapshotRepository = snapshotRepository;
        _recalculateUseCase = recalculateUseCase;
    }

    public async Task<ReadinessResponse> ExecuteAsync(Guid learnerId, CancellationToken ct = default)
    {
        var snapshot = await _snapshotRepository.GetLatestAsync(learnerId.ToString(), ct);
        
        // If no snapshot exists, calculate it on the fly
        ReadinessScore score;
        DateTimeOffset calculatedAt;
        
        if (snapshot is null)
        {
            score = await _recalculateUseCase.ExecuteAsync(learnerId.ToString(), ct);
            calculatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            score = new ReadinessScore(learnerId, snapshot.Score, snapshot.FormulaVersion, snapshot.GetComponents(), snapshot.CalculatedAt);
            calculatedAt = snapshot.CalculatedAt;
        }

        var capabilities = score.Components.Select(c => new ReadinessComponentResponse(
            c.Name,
            c.RawValue,
            c.Weight,
            c.WeightedValue,
            c.Explanation
        )).ToList();

        return new ReadinessResponse(
            score.Score,
            ReadinessFormula.ReadinessFormulaVersion,
            "Stable",
            calculatedAt,
            capabilities
        );
    }
}
