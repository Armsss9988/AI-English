using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishCoach.Domain.Curriculum;

namespace EnglishCoach.Infrastructure.Persistence.Configurations;

internal sealed class PhraseConfiguration : IEntityTypeConfiguration<Phrase>
{
    public void Configure(EntityTypeBuilder<Phrase> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Text)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.ViMeaning)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.CommunicationFunction)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Level)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Example)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.State)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.ContentVersion)
            .IsRequired();

        builder.Property(p => p.CreatedAtUtc)
            .IsRequired();

        builder.Property(p => p.UpdatedAtUtc)
            .IsRequired();
            
        // Used for querying
        builder.HasIndex(p => p.State);
        builder.HasIndex(p => p.CommunicationFunction);
        builder.HasIndex(p => p.Level);
    }
}
