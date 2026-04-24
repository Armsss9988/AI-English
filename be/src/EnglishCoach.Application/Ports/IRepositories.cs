using EnglishCoach.SharedKernel.Ids;

namespace EnglishCoach.Application.Ports;

public interface ISpeakingRepository
{
    Task<SpeakingAttempt?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SpeakingAttempt?> GetBySessionAsync(Guid sessionId, CancellationToken ct = default);
    Task AddAsync(SpeakingAttempt attempt, CancellationToken ct = default);
    Task UpdateAsync(SpeakingAttempt attempt, CancellationToken ct = default);
}

public interface IRoleplayRepository
{
    Task<RoleplaySession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(RoleplaySession session, CancellationToken ct = default);
    Task UpdateAsync(RoleplaySession session, CancellationToken ct = default);
}

public interface IReviewRepository
{
    Task<ReviewItem?> GetDueByLearnerAsync(Guid learnerId, int limit, CancellationToken ct = default);
    Task<ReviewItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ReviewItem item, CancellationToken ct = default);
    Task UpdateAsync(ReviewItem item, CancellationToken ct = default);
}

public interface IProgressRepository
{
    Task<ReadinessSnapshot?> GetLatestAsync(Guid learnerId, CancellationToken ct = default);
    Task AddSnapshotAsync(ReadinessSnapshot snapshot, CancellationToken ct = default);
}

// Domain entities (placeholder for now)
public class SpeakingAttempt
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Status { get; set; } = "created";
    public string? Transcript { get; set; }
    public string? Feedback { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? EvaluatedAt { get; set; }
}

public class RoleplaySession
{
    public Guid Id { get; set; }
    public Guid ScenarioId { get; set; }
    public string Status { get; set; } = "created";
    public DateTimeOffset CreatedAt { get; set; }
}

public class ReviewItem
{
    public Guid Id { get; set; }
    public Guid LearnerId { get; set; }
    public Guid PhraseId { get; set; }
    public string State { get; set; } = "new";
    public DateTimeOffset? DueAt { get; set; }
}

public class ReadinessSnapshot
{
    public Guid Id { get; set; }
    public Guid LearnerId { get; set; }
    public decimal Score { get; set; }
    public int FormulaVersion { get; set; }
    public DateTimeOffset CalculatedAt { get; set; }
}