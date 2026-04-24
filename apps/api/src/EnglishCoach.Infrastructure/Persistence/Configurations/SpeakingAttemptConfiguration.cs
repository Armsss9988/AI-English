using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishCoach.Domain.Speaking;

namespace EnglishCoach.Infrastructure.Persistence.Configurations;

internal sealed class SpeakingAttemptConfiguration : IEntityTypeConfiguration<SpeakingAttempt>
{
    public void Configure(EntityTypeBuilder<SpeakingAttempt> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.LearnerId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.ContentItemId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.State)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.AudioUrl)
            .HasMaxLength(500);

        builder.Property(a => a.RawTranscript)
            .HasMaxLength(4000);

        builder.Property(a => a.NormalizedTranscript)
            .HasMaxLength(4000);

        builder.OwnsOne(a => a.Feedback, f =>
        {
            f.Property(fb => fb.TopMistakes).HasColumnName("FeedbackTopMistakes").HasMaxLength(1000);
            f.Property(fb => fb.ImprovedAnswer).HasColumnName("FeedbackImprovedAnswer").HasMaxLength(1000);
            f.Property(fb => fb.PhrasesToReview).HasColumnName("FeedbackPhrasesToReview").HasMaxLength(1000);
            f.Property(fb => fb.RetryPrompt).HasColumnName("FeedbackRetryPrompt").HasMaxLength(1000);
        });

        builder.Property(a => a.CreatedAtUtc)
            .IsRequired();

        builder.Property(a => a.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(a => a.LearnerId);
        builder.HasIndex(a => a.ContentItemId);
    }
}
