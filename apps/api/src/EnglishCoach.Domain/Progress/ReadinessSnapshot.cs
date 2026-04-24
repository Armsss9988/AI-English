namespace EnglishCoach.Domain.Progress;

public class ReadinessSnapshotEntity
{
    public Guid Id { get; private set; }
    public Guid LearnerId { get; private set; }
    public decimal Score { get; private set; }
    public int FormulaVersion { get; private set; }
    public string ComponentsJson { get; private set; } = string.Empty;
    public DateTimeOffset CalculatedAt { get; private set; }

    private ReadinessSnapshotEntity() { } // EF Core

    public ReadinessSnapshotEntity(Guid learnerId, ReadinessScore score)
    {
        Id = Guid.NewGuid();
        LearnerId = learnerId;
        Score = score.Score;
        FormulaVersion = score.FormulaVersion;
        ComponentsJson = System.Text.Json.JsonSerializer.Serialize(score.Components);
        CalculatedAt = score.CalculatedAt;
    }

    public IReadOnlyList<ReadinessComponent> GetComponents()
    {
        return System.Text.Json.JsonSerializer.Deserialize<List<ReadinessComponent>>(ComponentsJson)
            ?? new List<ReadinessComponent>();
    }
}
