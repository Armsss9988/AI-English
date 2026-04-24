using EnglishCoach.Domain.LearningContent;
using EnglishCoach.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishCoach.Infrastructure.Seed;

/// <summary>
/// Database seeder for curriculum content.
/// Use in Program.cs or migration seed.
/// </summary>
public class DatabaseCurriculumSeeder
{
    private readonly EnglishCoachDbContext _context;

    public DatabaseCurriculumSeeder(EnglishCoachDbContext context)
    {
        _context = context;
    }

    public async Task SeedIfEmptyAsync(CancellationToken ct = default)
    {
        var hasContent = await _context.Set<ContentItem>().AnyAsync(ct);
        if (hasContent)
            return;

        var seeder = CurriculumSeeder.Seed();
        await _context.Set<ContentItem>().AddRangeAsync(seeder.Phrases, ct);
        await _context.Set<ContentItem>().AddRangeAsync(seeder.Scenarios, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> GetContentCountAsync(CancellationToken ct = default)
    {
        return await _context.Set<ContentItem>().CountAsync(ct);
    }

    public async Task<bool> HasContentAsync(CancellationToken ct = default)
    {
        return await _context.Set<ContentItem>().AnyAsync(ct);
    }
}