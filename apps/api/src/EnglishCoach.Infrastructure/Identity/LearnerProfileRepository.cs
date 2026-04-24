using Microsoft.EntityFrameworkCore;
using EnglishCoach.Application.Identity;
using EnglishCoach.Domain.Identity;
using EnglishCoach.Infrastructure.Persistence;

namespace EnglishCoach.Infrastructure.Identity;

public sealed class LearnerProfileRepository : ILearnerProfileRepository
{
    private readonly EnglishCoachDbContext _dbContext;

    public LearnerProfileRepository(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LearnerProfile?> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _dbContext.LearnerProfiles
            .SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public async Task CreateAsync(LearnerProfile profile, CancellationToken cancellationToken)
    {
        await _dbContext.LearnerProfiles.AddAsync(profile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LearnerProfile profile, CancellationToken cancellationToken)
    {
        _dbContext.LearnerProfiles.Update(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
