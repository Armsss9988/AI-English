using Microsoft.EntityFrameworkCore;
using EnglishCoach.Application.ErrorNotebook;
using EnglishCoach.Domain.ErrorNotebook;

namespace EnglishCoach.Infrastructure.Persistence.Repositories;

public sealed class NotebookRepository : INotebookRepository
{
    private readonly EnglishCoachDbContext _dbContext;

    public NotebookRepository(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotebookEntry?> GetByPatternKeyAsync(string learnerId, string patternKey, CancellationToken ct = default)
    {
        return await _dbContext.NotebookEntries
            .FirstOrDefaultAsync(e => e.LearnerId == learnerId && e.PatternKey == patternKey, ct);
    }

    public async Task<IReadOnlyList<NotebookEntry>> GetLearnerEntriesAsync(string learnerId, CancellationToken ct = default)
    {
        return await _dbContext.NotebookEntries
            .Where(e => e.LearnerId == learnerId)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(NotebookEntry entry, CancellationToken ct = default)
    {
        await _dbContext.NotebookEntries.AddAsync(entry, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(NotebookEntry entry, CancellationToken ct = default)
    {
        _dbContext.NotebookEntries.Update(entry);
        await _dbContext.SaveChangesAsync(ct);
    }
}
