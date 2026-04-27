using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Application.InterviewPractice;

public interface IInterviewSessionRepository
{
    Task<InterviewSession?> GetByIdAsync(string sessionId, CancellationToken ct = default);
    Task CreateAsync(InterviewSession session, CancellationToken ct = default);
    Task UpdateAsync(InterviewSession session, CancellationToken ct = default);
    Task<List<InterviewSession>> GetByLearnerIdAsync(string learnerId, CancellationToken ct = default);
}

public interface IInterviewProfileRepository
{
    Task<InterviewProfile?> GetByIdAsync(string profileId, CancellationToken ct = default);
    Task<InterviewProfile?> GetLatestByLearnerIdAsync(string learnerId, CancellationToken ct = default);
    Task CreateAsync(InterviewProfile profile, CancellationToken ct = default);
    Task UpdateAsync(InterviewProfile profile, CancellationToken ct = default);
}
