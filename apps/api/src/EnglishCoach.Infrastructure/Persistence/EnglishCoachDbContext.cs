using Microsoft.EntityFrameworkCore;
using EnglishCoach.Domain.Identity;

namespace EnglishCoach.Infrastructure.Persistence;

public sealed class EnglishCoachDbContext : DbContext
{
    public EnglishCoachDbContext(DbContextOptions<EnglishCoachDbContext> options)
        : base(options)
    {
    }

    public DbSet<LearnerProfile> LearnerProfiles => Set<LearnerProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnglishCoachDbContext).Assembly);
    }
}
