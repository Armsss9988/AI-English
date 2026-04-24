using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EnglishCoach.Contracts.Review;
using EnglishCoach.Domain.Review;
using EnglishCoach.Infrastructure.Persistence;

namespace EnglishCoach.ApiTests.Review;

public sealed class ReviewEndpointsTests : IAsyncLifetime
{
    private readonly SqliteConnection _database = new("Data Source=:memory:");
    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        await _database.OpenAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions<EnglishCoachDbContext>));
                    services.AddDbContext<EnglishCoachDbContext>(options => options.UseSqlite(_database));

                    using var scope = services.BuildServiceProvider().CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<EnglishCoachDbContext>();
                    dbContext.Database.EnsureCreated();
                });
            });
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetDueReviewItems_Returns_Stable_Empty_Payload()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-1");

        var response = await client.GetFromJsonAsync<GetDueReviewItemsResponse>("/me/reviews/due");

        Assert.NotNull(response);
        Assert.Empty(response!.Items);
    }

    [Fact]
    public async Task CompleteReviewItem_Writes_Attempt_And_Reschedules()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnglishCoachDbContext>();
        var seeded = ReviewItem.Create(
            "review-1",
            "user-1",
            "phrase-1",
            ReviewTrack.Phrase,
            "Could you please clarify?",
            "Yeu cau lam ro",
            DateTimeOffset.UtcNow.AddHours(-1),
            DateTimeOffset.UtcNow.AddDays(-1));
        dbContext.ReviewItems.Add(seeded);
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-1");

        var response = await client.PostAsJsonAsync(
            "/me/reviews/review-1/complete",
            new CompleteReviewItemRequest("good"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CompleteReviewItemResponse>();
        Assert.NotNull(payload);
        Assert.Equal("review-1", payload!.ReviewItemId);
        Assert.Equal("learning", payload.NextMasteryState);

        var attempts = await dbContext.ReviewAttempts.CountAsync();
        Assert.Equal(1, attempts);
    }
}
