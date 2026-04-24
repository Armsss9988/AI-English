using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EnglishCoach.Contracts.Identity;
using EnglishCoach.Domain.Identity;
using EnglishCoach.Infrastructure.Persistence;

namespace EnglishCoach.ApiTests.Identity;

public sealed class MyProfileEndpointsTests : IAsyncLifetime
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
    public async Task GetMyProfile_Returns_Authenticated_Users_Profile()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnglishCoachDbContext>();

        dbContext.LearnerProfiles.Add(LearnerProfile.Create(
            userId: "user-100",
            displayName: "Minh",
            nativeLanguage: "vi",
            timezone: "Asia/Bangkok",
            currentEnglishLevel: EnglishLevel.B1,
            targetUseCase: "client calls",
            targetTimelineWeeks: 10,
            role: LearnerRole.Dev));
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-100");

        var response = await client.GetFromJsonAsync<MyProfileResponse>("/me/profile");

        Assert.NotNull(response);
        Assert.Equal("user-100", response!.UserId);
        Assert.Equal("Minh", response.DisplayName);
        Assert.Equal("B1", response.CurrentEnglishLevel);
    }

    [Fact]
    public async Task UpdateMyProfile_Returns_BadRequest_For_Invalid_Payload()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnglishCoachDbContext>();

        dbContext.LearnerProfiles.Add(LearnerProfile.Create(
            userId: "user-101",
            displayName: "Lan",
            nativeLanguage: "vi",
            timezone: "UTC",
            currentEnglishLevel: EnglishLevel.A2,
            targetUseCase: "daily chat",
            targetTimelineWeeks: 8,
            role: LearnerRole.Qa));
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-101");

        var request = new UpdateMyProfileRequest(
            DisplayName: "",
            NativeLanguage: "vi",
            Timezone: "UTC",
            CurrentLevel: "B2",
            TargetUseCase: "support calls",
            TargetTimelineWeeks: 12,
            Role: "invalid-role");

        var response = await client.PutAsJsonAsync("/me/profile", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMyProfile_Updates_Allowed_Fields()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnglishCoachDbContext>();

        dbContext.LearnerProfiles.Add(LearnerProfile.Create(
            userId: "user-102",
            displayName: "Bao",
            nativeLanguage: "vi",
            timezone: "UTC",
            currentEnglishLevel: EnglishLevel.A2,
            targetUseCase: "emails",
            targetTimelineWeeks: 14,
            role: LearnerRole.Other));
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-102");

        var request = new UpdateMyProfileRequest(
            DisplayName: "Bao Nguyen",
            NativeLanguage: "vi",
            Timezone: "Asia/Bangkok",
            CurrentLevel: "B2",
            TargetUseCase: "technical interviews",
            TargetTimelineWeeks: 18,
            Role: "pm");

        var response = await client.PutAsJsonAsync("/me/profile", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<MyProfileResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Bao Nguyen", payload!.DisplayName);
        Assert.Equal("Asia/Bangkok", payload.Timezone);
        Assert.Equal("B2", payload.CurrentEnglishLevel);
        Assert.Equal("pm", payload.Role);
    }
}
