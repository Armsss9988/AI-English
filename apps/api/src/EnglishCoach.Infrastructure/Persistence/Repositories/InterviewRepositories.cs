using Microsoft.EntityFrameworkCore;
using EnglishCoach.Application.InterviewPractice;
using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Infrastructure.Persistence.Repositories;

public sealed class InterviewSessionRepository : IInterviewSessionRepository
{
    private readonly EnglishCoachDbContext _dbContext;

    public InterviewSessionRepository(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InterviewSession?> GetByIdAsync(string sessionId, CancellationToken ct = default)
    {
        return await _dbContext.InterviewSessions
            .Include(s => s.Turns)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);
    }

    public async Task CreateAsync(InterviewSession session, CancellationToken ct = default)
    {
        await _dbContext.InterviewSessions.AddAsync(session, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(InterviewSession session, CancellationToken ct = default)
    {
        _dbContext.InterviewSessions.Update(session);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<InterviewSession>> GetByLearnerIdAsync(string learnerId, CancellationToken ct = default)
    {
        return await _dbContext.InterviewSessions
            .Where(s => s.LearnerId == learnerId)
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(ct);
    }
}

public sealed class InterviewProfileRepository : IInterviewProfileRepository
{
    private readonly EnglishCoachDbContext _dbContext;

    public InterviewProfileRepository(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InterviewProfile?> GetByIdAsync(string profileId, CancellationToken ct = default)
    {
        return await _dbContext.InterviewProfiles.FirstOrDefaultAsync(p => p.Id == profileId, ct);
    }

    public async Task<InterviewProfile?> GetLatestByLearnerIdAsync(string learnerId, CancellationToken ct = default)
    {
        var profiles = await _dbContext.InterviewProfiles
            .Where(p => p.LearnerId == learnerId)
            .ToListAsync(ct);

        return profiles
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstOrDefault();
    }

    public async Task CreateAsync(InterviewProfile profile, CancellationToken ct = default)
    {
        await _dbContext.InterviewProfiles.AddAsync(profile, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(InterviewProfile profile, CancellationToken ct = default)
    {
        _dbContext.InterviewProfiles.Update(profile);
        await _dbContext.SaveChangesAsync(ct);
    }
}
