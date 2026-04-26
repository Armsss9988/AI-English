using EnglishCoach.Application.Progress;
using EnglishCoach.Domain.Progress;
using EnglishCoach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCoach.Infrastructure.Progress;

public class ProgressDataProvider : IProgressDataProvider, ILearnerProgressDataProvider
{
    private readonly EnglishCoachDbContext _dbContext;

    public ProgressDataProvider(EnglishCoachDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ILearnerProgressDataProvider
    public async Task<LearnerProgressData> GetLearnerProgressAsync(Guid learnerId, CancellationToken ct = default)
    {
        // Calculate average phrase mastery (from ReviewItems)
        var reviewItems = await _dbContext.ReviewItems
            .Where(r => r.UserId == learnerId.ToString())
            .ToListAsync(ct);
            
        var averageMastery = reviewItems.Count > 0 
            ? (decimal)reviewItems.Average(r => GetMasteryScore(r.RepetitionCount * 3)) // Dummy interval logic based on repetition count
            : 0m;

        // Critical error count
        var criticalErrorCount = await _dbContext.NotebookEntries
            .CountAsync(n => n.LearnerId == learnerId.ToString() && n.Severity == EnglishCoach.Domain.ErrorNotebook.ErrorSeverity.Critical, ct);

        // Completed roleplay scenarios
        var completedSessions = await _dbContext.RoleplaySessions
            .Where(s => s.LearnerId == learnerId.ToString() && s.State == EnglishCoach.Domain.Roleplay.RoleplaySessionState.Finalized)
            .Select(s => s.ScenarioId)
            .ToListAsync(ct);

        var scenarioTitles = await _dbContext.RoleplayScenarios
            .Where(s => completedSessions.Contains(s.Id))
            .Select(s => s.Title)
            .ToListAsync(ct);

        return new LearnerProgressData(
            learnerId,
            averageMastery,
            criticalErrorCount,
            scenarioTitles
        );
    }

    private decimal GetMasteryScore(int intervalDays)
    {
        // Simple heuristic: interval >= 21 days means 1.0 (100% mastery)
        return Math.Min(1m, intervalDays / 21m);
    }

    // IProgressDataProvider
    public async Task<decimal> GetReviewCompletionRateAsync(string learnerId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var dueDates = await _dbContext.ReviewItems
            .Where(r => r.UserId == learnerId)
            .Select(r => r.DueAtUtc)
            .ToListAsync(ct);
            
        var dueItemsCount = dueDates.Count(d => d <= now);
        
        var totalItemsCount = dueDates.Count;
            
        if (totalItemsCount == 0) return 0m;
        
        var completedItems = totalItemsCount - dueItemsCount;
        return (decimal)completedItems / totalItemsCount;
    }

    public async Task<decimal> GetPhraseMasteryAverageAsync(string learnerId, CancellationToken ct = default)
    {
        if (!Guid.TryParse(learnerId, out var id)) return 0m;
        var data = await GetLearnerProgressAsync(id, ct);
        return data.AveragePhraseMastery;
    }

    public async Task<decimal> GetSpeakingTaskCompletionRateAsync(string learnerId, CancellationToken ct = default)
    {
        var attempts = await _dbContext.SpeakingAttempts
            .Where(a => a.LearnerId == learnerId)
            .CountAsync(ct);
            
        // Dummy target: 10 speaking attempts is 100%
        return Math.Min(1m, attempts / 10m);
    }

    public async Task<decimal> GetRoleplaySuccessRateAsync(string learnerId, CancellationToken ct = default)
    {
        var completedSessions = await _dbContext.RoleplaySessions
            .Where(s => s.LearnerId == learnerId && s.State == EnglishCoach.Domain.Roleplay.RoleplaySessionState.Finalized)
            .CountAsync(ct);
            
        // Dummy target: 5 successful roleplays is 100%
        return Math.Min(1m, completedSessions / 5m);
    }

    public async Task<decimal> GetCriticalErrorCountAsync(string learnerId, CancellationToken ct = default)
    {
        if (!Guid.TryParse(learnerId, out var id)) return 0m;
        var data = await GetLearnerProgressAsync(id, ct);
        
        // Return inverse score based on errors (0 errors = 1.0, 5+ errors = 0.0)
        return Math.Max(0m, 1m - (data.CriticalErrorCount / 5m));
    }

    public async Task<decimal> GetRetrySuccessRateAsync(string learnerId, CancellationToken ct = default)
    {
        // Placeholder for retry success rate
        return 0.8m;
    }
}
