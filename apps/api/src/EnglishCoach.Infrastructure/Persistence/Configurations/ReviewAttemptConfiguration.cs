using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EnglishCoach.Domain.Review;

namespace EnglishCoach.Infrastructure.Persistence.Configurations;

public sealed class ReviewAttemptConfiguration : IEntityTypeConfiguration<ReviewAttempt>
{
    public void Configure(EntityTypeBuilder<ReviewAttempt> builder)
    {
        var dateTimeOffsetConverter = new ValueConverter<DateTimeOffset, long>(
            value => value.ToUnixTimeMilliseconds(),
            value => DateTimeOffset.FromUnixTimeMilliseconds(value));

        builder.ToTable("review_attempts");

        builder.HasKey(item => item.Id);
        builder.HasOne<ReviewItem>()
            .WithMany()
            .HasForeignKey(item => item.ReviewItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(item => item.Id).HasColumnName("id").HasMaxLength(64);
        builder.Property(item => item.ReviewItemId).HasColumnName("review_item_id").HasMaxLength(64).IsRequired();
        builder.Property(item => item.Quality).HasColumnName("quality").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(item => item.PreviousState).HasColumnName("previous_state").HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(item => item.NextState).HasColumnName("next_state").HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(item => item.PreviousRepetitionCount).HasColumnName("previous_repetition_count").IsRequired();
        builder.Property(item => item.NextRepetitionCount).HasColumnName("next_repetition_count").IsRequired();
        builder.Property(item => item.CompletedAtUtc).HasColumnName("completed_at_utc").HasConversion(dateTimeOffsetConverter).HasColumnType("bigint").IsRequired();
        builder.Property(item => item.NextDueAtUtc).HasColumnName("next_due_at_utc").HasConversion(dateTimeOffsetConverter).HasColumnType("bigint").IsRequired();
    }
}
