using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using EnglishCoach.Domain.Curriculum;
using EnglishCoach.Infrastructure.Persistence;
using EnglishCoach.Infrastructure.Seed;

namespace EnglishCoach.IntegrationTests.Curriculum;

public sealed class CurriculumSeederTests
{
    [Fact]
    public async Task SeedAsync_Does_Not_Duplicate_Existing_Phrase_And_Scenario_Content()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();

        var options = new DbContextOptionsBuilder<EnglishCoachDbContext>()
            .UseSqlite(database)
            .Options;

        await using var dbContext = new EnglishCoachDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var existingPhrase = CurriculumSeeder.GetSeedPhrases().First();
        var existingScenario = CurriculumSeeder.GetSeedScenarios().First();

        dbContext.Phrases.Add(existingPhrase);
        dbContext.RoleplayScenarios.Add(existingScenario);
        await dbContext.SaveChangesAsync();

        var seeder = new CurriculumSeeder(dbContext);

        await seeder.SeedAsync();

        var phraseTexts = await dbContext.Phrases.Select(phrase => phrase.Text).ToListAsync();
        var scenarioTitles = await dbContext.RoleplayScenarios.Select(scenario => scenario.Title).ToListAsync();

        Assert.Equal(phraseTexts.Count, phraseTexts.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(scenarioTitles.Count, scenarioTitles.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(CurriculumSeeder.GetSeedPhrases().Count, phraseTexts.Count);
        Assert.Equal(CurriculumSeeder.GetSeedScenarios().Count, scenarioTitles.Count);
    }
}
