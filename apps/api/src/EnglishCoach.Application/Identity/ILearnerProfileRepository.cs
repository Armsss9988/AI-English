using EnglishCoach.Domain.Identity;

namespace EnglishCoach.Application.Identity;

public interface ILearnerProfileRepository
{
    Task<LearnerProfile?> GetByUserIdAsync(string userId, CancellationToken cancellationToken);

    Task CreateAsync(LearnerProfile profile, CancellationToken cancellationToken);

    Task UpdateAsync(LearnerProfile profile, CancellationToken cancellationToken);
}
