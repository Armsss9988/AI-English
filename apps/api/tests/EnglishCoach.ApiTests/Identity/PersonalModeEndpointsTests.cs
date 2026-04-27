using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EnglishCoach.Contracts.Identity;
using EnglishCoach.Infrastructure.Persistence;

namespace EnglishCoach.ApiTests.Identity;

public sealed class PersonalModeEndpointsTests : IAsyncLifetime
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
    public async Task GetIdentity_WithoutHeaders_ReturnsDefaultPersonalLocalUser()
    {
        var client = _factory.CreateClient();

        var response = await client.GetFromJsonAsync<PersonalModeIdentityResponse>("/me/identity");

        Assert.NotNull(response);
        Assert.Equal("00000000-0000-0000-0000-000000000001", response!.UserId);
        Assert.Equal("personal-local", response.Mode);
        Assert.False(response.IsAdmin);
        Assert.Contains("intentionally deferred", response.Note);
    }

    [Fact]
    public async Task GetIdentity_WithExplicitUserId_ReturnsProvidedUser()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "custom-user-42");

        var response = await client.GetFromJsonAsync<PersonalModeIdentityResponse>("/me/identity");

        Assert.NotNull(response);
        Assert.Equal("custom-user-42", response!.UserId);
        Assert.Equal("personal-local", response.Mode);
        Assert.False(response.IsAdmin);
    }

    [Fact]
    public async Task GetIdentity_WithAdminRoleHeader_ReportsIsAdmin()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Role", "Admin");

        var response = await client.GetFromJsonAsync<PersonalModeIdentityResponse>("/me/identity");

        Assert.NotNull(response);
        Assert.True(response!.IsAdmin);
    }

    [Fact]
    public async Task GetIdentity_WithAdminRoleCaseInsensitive_ReportsIsAdmin()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Role", "admin");

        var response = await client.GetFromJsonAsync<PersonalModeIdentityResponse>("/me/identity");

        Assert.NotNull(response);
        Assert.True(response!.IsAdmin);
    }

    [Fact]
    public async Task GetProfile_WithoutUserIdHeader_UsesDefaultLocalUser()
    {
        // No X-User-Id header → should use default and return NotFound (no profile seeded for default user)
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/me/profile");

        // Default user has no profile seeded, so 404
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AdminRoute_CaseInsensitiveRoleHeader_Succeeds()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Role", "ADMIN");

        var response = await client.GetAsync("/admin/content/phrases");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
