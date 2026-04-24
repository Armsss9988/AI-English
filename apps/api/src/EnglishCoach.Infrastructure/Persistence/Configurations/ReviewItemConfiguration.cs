using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EnglishCoach.Domain.Review;

namespace EnglishCoach.Infrastructure.Persistence.Configurations;

public sealed class ReviewItemConfiguration : IEntityTypeConfiguration<ReviewItem>
{
    public void Configure(EntityTypeBuilder<ReviewItem> builder)
    {
        var dateTimeOffsetConverter = new ValueConverter<DateTimeOffset, long>(
            value => value.ToUnixTimeMilliseconds(),
            value => DateTimeOffset.FromUnixTimeMilliseconds(value));
        var nullableDateTimeOffsetConverter = new ValueConverter<DateTimeOffset?, long?>(
            value => value.HasValue ? value.Value.ToUnixTimeMilliseconds() : null,
            value => value.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(value.Value) : null);

        builder.ToTable("review_items");

        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.UserId, item.ItemId, item.ReviewTrack }).IsUnique();

        builder.Property(item => item.Id).HasColumnName("id").HasMaxLength(64);
        builder.Property(item => item.UserId).HasColumnName("user_id").HasMaxLength(128).IsRequired();
        builder.Property(item => item.ItemId).HasColumnName("item_id").HasMaxLength(128).IsRequired();
        builder.Property(item => item.ReviewTrack).HasColumnName("review_track").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(item => item.DisplayText).HasColumnName("display_text").HasMaxLength(240).IsRequired();
        builder.Property(item => item.DisplaySubtitle).HasColumnName("display_subtitle").HasMaxLength(240);
        builder.Property(item => item.MasteryState).HasColumnName("mastery_state").HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(item => item.RepetitionCount).HasColumnName("repetition_count").IsRequired();
        builder.Property(item => item.DueAtUtc).HasColumnName("due_at_utc").HasConversion(dateTimeOffsetConverter).HasColumnType("bigint").IsRequired();
        builder.Property(item => item.CreatedAtUtc).HasColumnName("created_at_utc").HasConversion(dateTimeOffsetConverter).HasColumnType("bigint").IsRequired();
        builder.Property(item => item.UpdatedAtUtc).HasColumnName("updated_at_utc").HasConversion(dateTimeOffsetConverter).HasColumnType("bigint").IsRequired();
        builder.Property(item => item.LastCompletedAtUtc).HasColumnName("last_completed_at_utc").HasConversion(nullableDateTimeOffsetConverter).HasColumnType("bigint");
    }
}
