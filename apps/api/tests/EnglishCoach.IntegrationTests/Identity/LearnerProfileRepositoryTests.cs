using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using EnglishCoach.Domain.Identity;
using EnglishCoach.Infrastructure.Identity;
using EnglishCoach.Infrastructure.Persistence;

namespace EnglishCoach.IntegrationTests.Identity;

public sealed class LearnerProfileRepositoryTests
{
    [Fact]
    public async Task CreateAsync_Persists_And_Reads_Profile()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();

        var options = new DbContextOptionsBuilder<EnglishCoachDbContext>()
            .UseSqlite(database)
            .Options;

        await using var dbContext = new EnglishCoachDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new LearnerProfileRepository(dbContext);
        var profile = LearnerProfile.Create(
            userId: "user-1",
            displayName: "Minh",
            nativeLanguage: "vi",
            timezone: "Asia/Bangkok",
            currentEnglishLevel: EnglishLevel.B1,
            targetUseCase: "client calls",
            targetTimelineWeeks: 12,
            role: LearnerRole.Dev);

        await repository.CreateAsync(profile, CancellationToken.None);

        var reloaded = await repository.GetByUserIdAsync("user-1", CancellationToken.None);

        Assert.NotNull(reloaded);
        Assert.Equal("Minh", reloaded!.DisplayName);
        Assert.Equal("Asia/Bangkok", reloaded.Timezone);
        Assert.Equal(LearnerRole.Dev, reloaded.Role);
    }

    [Fact]
    public async Task UpdateAsync_Persists_Profile_Changes()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();

        var options = new DbContextOptionsBuilder<EnglishCoachDbContext>()
            .UseSqlite(database)
            .Options;

        await using var dbContext = new EnglishCoachDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new LearnerProfileRepository(dbContext);
        var profile = LearnerProfile.Create(
            userId: "user-2",
            displayName: "Lan",
            nativeLanguage: "vi",
            timezone: "Asia/Ho_Chi_Minh",
            currentEnglishLevel: EnglishLevel.A2,
            targetUseCase: "daily chat",
            targetTimelineWeeks: 16,
            role: LearnerRole.Qa);

        await repository.CreateAsync(profile, CancellationToken.None);

        profile.Update(
            displayName: "Lan Anh",
            nativeLanguage: "vi",
            timezone: "UTC",
            currentEnglishLevel: EnglishLevel.B2,
            targetUseCase: "technical interviews",
            targetTimelineWeeks: 20,
            role: LearnerRole.Pm);

        await repository.UpdateAsync(profile, CancellationToken.None);

        var reloaded = await repository.GetByUserIdAsync("user-2", CancellationToken.None);

        Assert.NotNull(reloaded);
        Assert.Equal("Lan Anh", reloaded!.DisplayName);
        Assert.Equal("UTC", reloaded.Timezone);
        Assert.Equal(EnglishLevel.B2, reloaded.CurrentEnglishLevel);
        Assert.Equal(LearnerRole.Pm, reloaded.Role);
    }
}
