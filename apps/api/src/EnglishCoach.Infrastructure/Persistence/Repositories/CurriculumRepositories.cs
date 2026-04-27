using Microsoft.EntityFrameworkCore;
using EnglishCoach.Application.Curriculum;
using EnglishCoach.Domain.Curriculum;

namespace EnglishCoach.Infrastructure.Persistence.Repositories;

public sealed class PhraseRepository : IPhraseRepository
{
    private readonly EnglishCoachDbContext _dbContext;

    public PhraseRepository(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Phrase?> GetByIdAsync(string phraseId, CancellationToken ct = default)
    {
        return await _dbContext.Phrases
            .FirstOrDefaultAsync(p => p.Id == phraseId, ct);
    }

    public async Task<IReadOnlyList<Phrase>> GetPublishedByFunctionAndLevelAsync(
        CommunicationFunction? function,
        ContentLevel? level,
        CancellationToken ct = default)
    {
        var query = _dbContext.Phrases.Where(p => p.State == ContentPublicationState.Published);

        if (function.HasValue)
            query = query.Where(p => p.CommunicationFunction == function.Value);
            
        if (level.HasValue)
            query = query.Where(p => p.Level == level.Value);

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Phrase>> GetAllPublishedAsync(CancellationToken ct = default)
    {
        return await _dbContext.Phrases
            .Where(p => p.State == ContentPublicationState.Published)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Phrase>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.Phrases.OrderByDescending(p => p.UpdatedAtUtc).ToListAsync(ct);
    }

    public async Task CreateAsync(Phrase phrase, CancellationToken ct = default)
    {
        await _dbContext.Phrases.AddAsync(phrase, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Phrase phrase, CancellationToken ct = default)
    {
        _dbContext.Phrases.Update(phrase);
        await _dbContext.SaveChangesAsync(ct);
    }
}

public sealed class RoleplayScenarioRepository : IRoleplayScenarioRepository
{
    private readonly EnglishCoachDbContext _dbContext;

    public RoleplayScenarioRepository(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoleplayScenario?> GetByIdAsync(string scenarioId, CancellationToken ct = default)
    {
        return await _dbContext.RoleplayScenarios
            .FirstOrDefaultAsync(s => s.Id == scenarioId, ct);
    }

    public async Task<IReadOnlyList<RoleplayScenario>> GetAllPublishedAsync(CancellationToken ct = default)
    {
        return await _dbContext.RoleplayScenarios
            .Where(s => s.State == ContentPublicationState.Published)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<RoleplayScenario>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.RoleplayScenarios.OrderByDescending(s => s.UpdatedAtUtc).ToListAsync(ct);
    }

    public async Task CreateAsync(RoleplayScenario scenario, CancellationToken ct = default)
    {
        await _dbContext.RoleplayScenarios.AddAsync(scenario, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RoleplayScenario scenario, CancellationToken ct = default)
    {
        _dbContext.RoleplayScenarios.Update(scenario);
        await _dbContext.SaveChangesAsync(ct);
    }
}
