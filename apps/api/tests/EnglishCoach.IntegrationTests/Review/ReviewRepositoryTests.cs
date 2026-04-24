using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using EnglishCoach.Application.Review;
using EnglishCoach.Contracts.Review;
using EnglishCoach.Domain.Review;
using EnglishCoach.Infrastructure.Persistence;
using EnglishCoach.Infrastructure.Review;

namespace EnglishCoach.IntegrationTests.Review;

public sealed class ReviewRepositoryTests
{
    [Fact]
    public async Task EnsureReviewItemExists_Is_Idempotent_For_Same_Key()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();

        var options = new DbContextOptionsBuilder<EnglishCoachDbContext>()
            .UseSqlite(database)
            .Options;

        await using var dbContext = new EnglishCoachDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new ReviewRepository(dbContext);
        var clock = new FakeClock(new DateTimeOffset(2026, 4, 24, 9, 0, 0, TimeSpan.Zero));
        var useCase = new EnsureReviewItemExistsUseCase(repository, clock);

        var first = await useCase.ExecuteAsync(new EnsureReviewItemRequest(
            "user-1",
            "phrase-1",
            "phrase",
            "Could you please clarify?",
            "Yeu cau lam ro"), CancellationToken.None);

        var second = await useCase.ExecuteAsync(new EnsureReviewItemRequest(
            "user-1",
            "phrase-1",
            "phrase",
            "Could you please clarify?",
            "Yeu cau lam ro"), CancellationToken.None);

        Assert.Equal(first.ReviewItemId, second.ReviewItemId);
        Assert.Equal(1, await dbContext.ReviewItems.CountAsync());
    }

    [Fact]
    public async Task GetDueReviewItems_Filters_By_User_And_Due_Timestamp()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();

        var options = new DbContextOptionsBuilder<EnglishCoachDbContext>()
            .UseSqlite(database)
            .Options;

        await using var dbContext = new EnglishCoachDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new ReviewRepository(dbContext);
        var clock = new FakeClock(new DateTimeOffset(2026, 4, 24, 9, 0, 0, TimeSpan.Zero));

        var dueItem = ReviewItem.Create(
            "item-1",
            "user-1",
            "phrase-1",
            ReviewTrack.Phrase,
            "phrase one",
            "subtitle",
            clock.UtcNow.AddHours(-1),
            clock.UtcNow);

        var futureItem = ReviewItem.Create(
            "item-2",
            "user-1",
            "phrase-2",
            ReviewTrack.Phrase,
            "phrase two",
            "subtitle",
            clock.UtcNow.AddHours(3),
            clock.UtcNow);

        var otherUserItem = ReviewItem.Create(
            "item-3",
            "user-2",
            "phrase-3",
            ReviewTrack.Phrase,
            "phrase three",
            "subtitle",
            clock.UtcNow.AddHours(-2),
            clock.UtcNow);

        await repository.CreateAsync(dueItem, CancellationToken.None);
        await repository.CreateAsync(futureItem, CancellationToken.None);
        await repository.CreateAsync(otherUserItem, CancellationToken.None);

        var items = await repository.GetDueItemsAsync("user-1", clock.UtcNow, CancellationToken.None);

        var single = Assert.Single(items);
        Assert.Equal("item-1", single.ReviewItemId);
        Assert.Equal("phrase one", single.DisplayText);
    }
}
