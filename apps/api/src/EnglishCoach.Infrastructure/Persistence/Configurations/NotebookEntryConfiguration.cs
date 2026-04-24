using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishCoach.Domain.ErrorNotebook;

namespace EnglishCoach.Infrastructure.Persistence.Configurations;

internal sealed class NotebookEntryConfiguration : IEntityTypeConfiguration<NotebookEntry>
{
    public void Configure(EntityTypeBuilder<NotebookEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.LearnerId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.PatternKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.OriginalExample)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.CorrectedExample)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.ExplanationVi)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.RecurrenceCount)
            .IsRequired();

        builder.Property(e => e.State)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.UpdatedAtUtc)
            .IsRequired();

        builder.Property(e => e.EvidenceRefs)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<NotebookEvidence>>(v, (System.Text.Json.JsonSerializerOptions?)null)!
            )
            .Metadata.SetValueComparer(
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyList<NotebookEvidence>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList().AsReadOnly()
                )
            );

        // Used for querying
        builder.HasIndex(e => new { e.LearnerId, e.PatternKey }).IsUnique();
        builder.HasIndex(e => e.State);
    }
}
