using Microsoft.EntityFrameworkCore;
using EnglishCoach.Domain.Identity;
using EnglishCoach.Domain.Review;

namespace EnglishCoach.Infrastructure.Persistence;

public sealed class EnglishCoachDbContext : DbContext
{
    public EnglishCoachDbContext(DbContextOptions<EnglishCoachDbContext> options)
        : base(options)
    {
    }

    public DbSet<LearnerProfile> LearnerProfiles => Set<LearnerProfile>();
    public DbSet<ReviewItem> ReviewItems => Set<ReviewItem>();
    public DbSet<ReviewAttempt> ReviewAttempts => Set<ReviewAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnglishCoachDbContext).Assembly);
    }
}
