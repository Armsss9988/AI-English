using EnglishCoach.Application.Ports;

namespace EnglishCoach.Infrastructure.Repositories;

public class SpeakingRepository : ISpeakingRepository
{
    public Task<SpeakingAttempt?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult<SpeakingAttempt?>(null);

    public Task<SpeakingAttempt?> GetBySessionAsync(Guid sessionId, CancellationToken ct = default) =>
        Task.FromResult<SpeakingAttempt?>(null);

    public Task AddAsync(SpeakingAttempt attempt, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task UpdateAsync(SpeakingAttempt attempt, CancellationToken ct = default) =>
        Task.CompletedTask;
}

public class RoleplayRepository : IRoleplayRepository
{
    public Task<RoleplaySession?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult<RoleplaySession?>(null);

    public Task AddAsync(RoleplaySession session, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task UpdateAsync(RoleplaySession session, CancellationToken ct = default) =>
        Task.CompletedTask;
}

public class ReviewRepository : IReviewRepository
{
    public Task<ReviewItem?> GetDueByLearnerAsync(Guid learnerId, int limit, CancellationToken ct = default) =>
        Task.FromResult<ReviewItem?>(null);

    public Task<ReviewItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult<ReviewItem?>(null);

    public Task AddAsync(ReviewItem item, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task UpdateAsync(ReviewItem item, CancellationToken ct = default) =>
        Task.CompletedTask;
}

public class ProgressRepository : IProgressRepository
{
    public Task<ReadinessSnapshot?> GetLatestAsync(Guid learnerId, CancellationToken ct = default) =>
        Task.FromResult<ReadinessSnapshot?>(null);

    public Task AddSnapshotAsync(ReadinessSnapshot snapshot, CancellationToken ct = default) =>
        Task.CompletedTask;
}