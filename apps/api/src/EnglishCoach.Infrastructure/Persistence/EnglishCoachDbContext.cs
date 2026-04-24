using Microsoft.EntityFrameworkCore;
using EnglishCoach.Domain.Identity;
using EnglishCoach.Domain.Review;
using EnglishCoach.Domain.Curriculum;
using EnglishCoach.Domain.ErrorNotebook;
using EnglishCoach.Domain.Roleplay;
using EnglishCoach.Domain.Speaking;

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
    public DbSet<Phrase> Phrases => Set<Phrase>();
    public DbSet<RoleplayScenario> RoleplayScenarios => Set<RoleplayScenario>();
    public DbSet<NotebookEntry> NotebookEntries => Set<NotebookEntry>();
    public DbSet<RoleplaySession> RoleplaySessions => Set<RoleplaySession>();
    public DbSet<RoleplayTurn> RoleplayTurns => Set<RoleplayTurn>();
    public DbSet<SpeakingAttempt> SpeakingAttempts => Set<SpeakingAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnglishCoachDbContext).Assembly);
    }
}
