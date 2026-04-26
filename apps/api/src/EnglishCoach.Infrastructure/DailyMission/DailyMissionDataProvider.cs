using EnglishCoach.Domain.DailyMission;
using EnglishCoach.Domain.Review;
using EnglishCoach.Domain.Curriculum;
using EnglishCoach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCoach.Infrastructure.DailyMission;

public class DailyMissionDataProvider : IDailyMissionDataProvider
{
    private readonly EnglishCoachDbContext _dbContext;

    public DailyMissionDataProvider(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DueReviewItem>> GetDueReviewsAsync(Guid learnerId, int limit, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var allItems = await _dbContext.ReviewItems
            .Where(r => r.UserId == learnerId.ToString())
            .ToListAsync(ct);

        var dueItems = allItems
            .Where(r => r.DueAtUtc <= now)
            .OrderBy(r => r.DueAtUtc)
            .Take(limit)
            .ToList();

        return dueItems.Select(r => new DueReviewItem(
            Guid.Parse(r.Id),
            Guid.Parse(r.ItemId),
            r.DisplayText,
            "Vocabulary",
            r.DueAtUtc
        )).ToList();
    }

    public async Task<IReadOnlyList<SpeakingTask>> GetSpeakingDrillsAsync(int limit, CancellationToken ct = default)
    {
        var phrases = await _dbContext.Phrases
            .OrderBy(r => EF.Functions.Random())
            .Take(limit)
            .ToListAsync(ct);

        return phrases.Select(p => new SpeakingTask(
            Guid.Parse(p.Id),
            p.Text,
            p.CommunicationFunction.ToString(),
            EnglishCoach.Domain.LearningContent.ContentType.Phrase
        )).ToList();
    }

    public async Task<IReadOnlyList<RoleplayTask>> GetRoleplayScenariosAsync(string? excludeGroup, int limit, CancellationToken ct = default)
    {
        var query = _dbContext.RoleplayScenarios.AsQueryable();

        var scenarios = await query
            .OrderBy(r => EF.Functions.Random())
            .Take(limit)
            .ToListAsync(ct);

        return scenarios.Select(s => new RoleplayTask(
            Guid.Parse(s.Id),
            s.Title,
            s.CommunicationGoal,
            s.ClientPersona,
            s.WorkplaceContext
        )).ToList();
    }

    public async Task<int> GetCriticalErrorCountAsync(Guid learnerId, int recentDays, CancellationToken ct = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-recentDays);
        var createdDates = await _dbContext.NotebookEntries
            .Where(n => n.LearnerId == learnerId.ToString() && n.Severity == EnglishCoach.Domain.ErrorNotebook.ErrorSeverity.Critical)
            .Select(n => n.CreatedAtUtc)
            .ToListAsync(ct);
            
        return createdDates.Count(d => d >= cutoff);
    }

    public async Task<IReadOnlyList<RetryTask>> GetRecentCriticalErrorsAsync(Guid learnerId, int limit, CancellationToken ct = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-7);
        var entries = await _dbContext.NotebookEntries
            .Where(n => n.LearnerId == learnerId.ToString() && n.Severity == EnglishCoach.Domain.ErrorNotebook.ErrorSeverity.Critical)
            .ToListAsync(ct);

        var recentEntries = entries
            .Where(n => n.CreatedAtUtc >= cutoff)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(limit)
            .ToList();

        return recentEntries.Select(e => new RetryTask(
            Guid.Parse(e.Id),
            e.PatternKey,
            e.CorrectedExample,
            e.Category.ToString()
        )).ToList();
    }
}
