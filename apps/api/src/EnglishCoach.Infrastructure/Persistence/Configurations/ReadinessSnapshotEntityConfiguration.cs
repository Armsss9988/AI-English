using EnglishCoach.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishCoach.Infrastructure.Persistence.Configurations;

public class ReadinessSnapshotEntityConfiguration : IEntityTypeConfiguration<ReadinessSnapshotEntity>
{
    public void Configure(EntityTypeBuilder<ReadinessSnapshotEntity> builder)
    {
        builder.ToTable("ReadinessSnapshots");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.LearnerId)
            .IsRequired();

        builder.Property(r => r.Score)
            .HasColumnType("decimal(7,4)")
            .IsRequired();

        builder.Property(r => r.FormulaVersion)
            .IsRequired();

        builder.Property(r => r.ComponentsJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(r => r.CalculatedAt)
            .IsRequired();

        builder.HasIndex(r => r.LearnerId);
    }
}
