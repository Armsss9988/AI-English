using EnglishCoach.Application.Progress;
using EnglishCoach.Domain.Progress;
using EnglishCoach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCoach.Infrastructure.Progress;

public sealed class ReadinessSnapshotRepository : IReadinessSnapshotRepository
{
    private readonly EnglishCoachDbContext _dbContext;

    public ReadinessSnapshotRepository(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ReadinessSnapshotEntity snapshot, CancellationToken ct = default)
    {
        await _dbContext.Set<ReadinessSnapshotEntity>().AddAsync(snapshot, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<ReadinessSnapshotEntity?> GetLatestAsync(string learnerId, CancellationToken ct = default)
    {
        if (!Guid.TryParse(learnerId, out var id))
            return null;

        return await _dbContext.Set<ReadinessSnapshotEntity>()
            .Where(s => s.LearnerId == id)
            .OrderByDescending(s => s.CalculatedAt)
            .FirstOrDefaultAsync(ct);
    }
}
