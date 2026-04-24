using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishCoach.Domain.Curriculum;

namespace EnglishCoach.Infrastructure.Persistence.Configurations;

internal sealed class RoleplayScenarioConfiguration : IEntityTypeConfiguration<RoleplayScenario>
{
    public void Configure(EntityTypeBuilder<RoleplayScenario> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.WorkplaceContext)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(s => s.UserRole)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ClientPersona)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.CommunicationGoal)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(s => s.Difficulty)
            .IsRequired();

        builder.Property(s => s.ContentVersion)
            .IsRequired();

        builder.Property(s => s.State)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.CreatedAtUtc)
            .IsRequired();

        builder.Property(s => s.UpdatedAtUtc)
            .IsRequired();

        builder.Property(s => s.MustCoverPoints)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!
            )
            .Metadata.SetValueComparer(
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyList<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList().AsReadOnly()
                )
            );

        builder.Property(s => s.TargetPhraseIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!
            )
            .Metadata.SetValueComparer(
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyList<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList().AsReadOnly()
                )
            );

        builder.Property(s => s.PassCriteria)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!
            )
            .Metadata.SetValueComparer(
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyList<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList().AsReadOnly()
                )
            );
            
        // Used for querying
        builder.HasIndex(s => s.State);
    }
}
