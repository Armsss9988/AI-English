using EnglishCoach.Domain.Roleplay;

namespace EnglishCoach.Application.Roleplay;

public interface IRoleplaySessionRepository
{
    Task<RoleplaySession?> GetByIdAsync(string sessionId, CancellationToken ct = default);
    Task CreateAsync(RoleplaySession session, CancellationToken ct = default);
    Task UpdateAsync(RoleplaySession session, CancellationToken ct = default);
}
