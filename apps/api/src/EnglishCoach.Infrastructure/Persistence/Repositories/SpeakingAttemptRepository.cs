using Microsoft.EntityFrameworkCore;
using EnglishCoach.Application.Speaking;
using EnglishCoach.Domain.Speaking;

namespace EnglishCoach.Infrastructure.Persistence.Repositories;

public sealed class SpeakingAttemptRepository : ISpeakingAttemptRepository
{
    private readonly EnglishCoachDbContext _dbContext;

    public SpeakingAttemptRepository(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SpeakingAttempt?> GetByIdAsync(string attemptId, CancellationToken ct = default)
    {
        return await _dbContext.SpeakingAttempts
            .FirstOrDefaultAsync(a => a.Id == attemptId, ct);
    }

    public async Task CreateAsync(SpeakingAttempt attempt, CancellationToken ct = default)
    {
        await _dbContext.SpeakingAttempts.AddAsync(attempt, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(SpeakingAttempt attempt, CancellationToken ct = default)
    {
        _dbContext.SpeakingAttempts.Update(attempt);
        await _dbContext.SaveChangesAsync(ct);
    }
}
