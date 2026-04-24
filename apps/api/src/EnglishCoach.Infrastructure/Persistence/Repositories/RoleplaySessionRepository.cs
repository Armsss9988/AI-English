using Microsoft.EntityFrameworkCore;
using EnglishCoach.Application.Roleplay;
using EnglishCoach.Domain.Roleplay;

namespace EnglishCoach.Infrastructure.Persistence.Repositories;

public sealed class RoleplaySessionRepository : IRoleplaySessionRepository
{
    private readonly EnglishCoachDbContext _dbContext;

    public RoleplaySessionRepository(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoleplaySession?> GetByIdAsync(string sessionId, CancellationToken ct = default)
    {
        return await _dbContext.RoleplaySessions
            .Include(s => s.Turns)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);
    }

    public async Task CreateAsync(RoleplaySession session, CancellationToken ct = default)
    {
        await _dbContext.RoleplaySessions.AddAsync(session, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RoleplaySession session, CancellationToken ct = default)
    {
        _dbContext.RoleplaySessions.Update(session);
        await _dbContext.SaveChangesAsync(ct);
    }
}
