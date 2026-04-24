using EnglishCoach.Domain.Curriculum;

namespace EnglishCoach.Application.Curriculum;

public interface IPhraseRepository
{
    Task<Phrase?> GetByIdAsync(string phraseId, CancellationToken ct = default);
    Task<IReadOnlyList<Phrase>> GetPublishedByFunctionAndLevelAsync(
        CommunicationFunction? function, ContentLevel? level, CancellationToken ct = default);
    Task<IReadOnlyList<Phrase>> GetAllPublishedAsync(CancellationToken ct = default);
    Task CreateAsync(Phrase phrase, CancellationToken ct = default);
    Task UpdateAsync(Phrase phrase, CancellationToken ct = default);
}

public interface IRoleplayScenarioRepository
{
    Task<RoleplayScenario?> GetByIdAsync(string scenarioId, CancellationToken ct = default);
    Task<IReadOnlyList<RoleplayScenario>> GetAllPublishedAsync(CancellationToken ct = default);
    Task CreateAsync(RoleplayScenario scenario, CancellationToken ct = default);
    Task UpdateAsync(RoleplayScenario scenario, CancellationToken ct = default);
}
