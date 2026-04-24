using Microsoft.EntityFrameworkCore;

namespace EnglishCoach.Infrastructure.Persistence;

public class EnglishCoachDbContext : DbContext
{
    public EnglishCoachDbContext(DbContextOptions<EnglishCoachDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities here
        base.OnModelCreating(modelBuilder);
    }
}