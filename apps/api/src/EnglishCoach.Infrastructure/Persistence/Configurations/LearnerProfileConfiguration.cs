using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishCoach.Domain.Identity;

namespace EnglishCoach.Infrastructure.Persistence.Configurations;

public sealed class LearnerProfileConfiguration : IEntityTypeConfiguration<LearnerProfile>
{
    public void Configure(EntityTypeBuilder<LearnerProfile> builder)
    {
        builder.ToTable("learner_profiles");

        builder.HasKey(profile => profile.UserId);

        builder.Property(profile => profile.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(128);

        builder.Property(profile => profile.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(profile => profile.NativeLanguage)
            .HasColumnName("native_language")
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(profile => profile.Timezone)
            .HasColumnName("timezone")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(profile => profile.CurrentEnglishLevel)
            .HasColumnName("current_english_level")
            .HasConversion<string>()
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(profile => profile.TargetUseCase)
            .HasColumnName("target_use_case")
            .HasMaxLength(240)
            .IsRequired();

        builder.Property(profile => profile.TargetTimelineWeeks)
            .HasColumnName("target_timeline_weeks")
            .IsRequired();

        builder.Property(profile => profile.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
    }
}
