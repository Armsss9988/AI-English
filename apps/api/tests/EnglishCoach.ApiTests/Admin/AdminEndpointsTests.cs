using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EnglishCoach.Infrastructure.Persistence;

namespace EnglishCoach.ApiTests.Admin;

public sealed class AdminEndpointsTests : IAsyncLifetime
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
    public async Task Admin_Route_Returns_Forbid_When_No_Role_Header()
    {
        var client = _factory.CreateClient();
        
        var response = await client.GetAsync("/admin/content/phrases");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Route_Returns_Forbid_When_Role_Is_Not_Admin()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Role", "Learner");
        
        var response = await client.GetAsync("/admin/content/phrases");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Route_Returns_Ok_When_Role_Is_Admin()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Role", "Admin");
        
        var response = await client.GetAsync("/admin/content/phrases");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
