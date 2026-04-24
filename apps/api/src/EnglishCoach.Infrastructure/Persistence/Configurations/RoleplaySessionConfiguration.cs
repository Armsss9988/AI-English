using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishCoach.Domain.Roleplay;

namespace EnglishCoach.Infrastructure.Persistence.Configurations;

internal sealed class RoleplayTurnConfiguration : IEntityTypeConfiguration<RoleplayTurn>
{
    public void Configure(EntityTypeBuilder<RoleplayTurn> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.SessionId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Message)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(t => t.AudioUrl)
            .HasMaxLength(500);

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();
            
        // No index needed unless we query turns individually, usually loaded via Session
    }
}

internal sealed class RoleplaySessionConfiguration : IEntityTypeConfiguration<RoleplaySession>
{
    public void Configure(EntityTypeBuilder<RoleplaySession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.LearnerId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.ScenarioId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.State)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.OwnsOne(s => s.Summary, summary =>
        {
            summary.Property(s => s.Result).HasColumnName("SummaryResult").HasMaxLength(50);
            summary.Property(s => s.ClearPoints).HasColumnName("SummaryClearPoints").HasMaxLength(1000);
            summary.Property(s => s.TopMistakes).HasColumnName("SummaryTopMistakes").HasMaxLength(1000);
            summary.Property(s => s.ImprovedAnswer).HasColumnName("SummaryImprovedAnswer").HasMaxLength(1000);
            summary.Property(s => s.PhrasesToReview).HasColumnName("SummaryPhrasesToReview").HasMaxLength(1000);
            summary.Property(s => s.RetryChallenge).HasColumnName("SummaryRetryChallenge").HasMaxLength(1000);
        });

        // 1-to-many relationship
        builder.HasMany(s => s.Turns)
            .WithOne()
            .HasForeignKey(t => t.SessionId)
            .OnDelete(DeleteBehavior.Cascade)
            .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(s => s.CreatedAtUtc)
            .IsRequired();

        builder.Property(s => s.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(s => s.LearnerId);
    }
}
