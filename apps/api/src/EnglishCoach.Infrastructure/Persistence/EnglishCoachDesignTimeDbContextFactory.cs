using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnglishCoach.Infrastructure.Persistence;

public sealed class EnglishCoachDesignTimeDbContextFactory : IDesignTimeDbContextFactory<EnglishCoachDbContext>
{
    public EnglishCoachDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EnglishCoachDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=englishcoach_dev;Username=postgres;Password=postgres");

        return new EnglishCoachDbContext(optionsBuilder.Options);
    }
}
