using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Infrastructure.Persistence.Configurations;

internal sealed class InterviewProfileConfiguration : IEntityTypeConfiguration<InterviewProfile>
{
    public void Configure(EntityTypeBuilder<InterviewProfile> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.LearnerId).IsRequired().HasMaxLength(50);
        builder.Property(p => p.CvText).IsRequired();
        builder.Property(p => p.CvAnalysis).HasDefaultValue(string.Empty);
        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc).IsRequired();
        builder.HasIndex(p => p.LearnerId);
    }
}

internal sealed class InterviewTurnConfiguration : IEntityTypeConfiguration<InterviewTurn>
{
    public void Configure(EntityTypeBuilder<InterviewTurn> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.SessionId).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Role).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.Message).IsRequired();
        builder.Property(t => t.AudioUrl).HasMaxLength(1000);
        builder.Property(t => t.TurnOrder).IsRequired();
        builder.Property(t => t.QuestionCategory).HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.CreatedAtUtc).IsRequired();

        // ── T01: Adaptive interview domain fields ──
        builder.Property(t => t.TurnType).HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.TargetCapability).HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.TurnState).HasConversion<string>().HasMaxLength(50)
            .HasDefaultValue(InterviewTurnState.Created);
        builder.Property(t => t.VerificationStatus).HasConversion<string>().HasMaxLength(50)
            .HasDefaultValue(InterviewVerificationStatus.Unverified);

        // ── T05/T06: Audio and transcript fields ──
        builder.Property(t => t.AudioStorageKey).HasMaxLength(500).HasDefaultValue(string.Empty);
        builder.Property(t => t.AudioDurationMs).HasDefaultValue(0);
        builder.Property(t => t.RawTranscript).HasDefaultValue(string.Empty);
        builder.Property(t => t.ConfirmedTranscript).HasDefaultValue(string.Empty);
        builder.Property(t => t.TranscriptConfidence).HasDefaultValue(0.0);
        builder.Property(t => t.LearnerEditedTranscript).HasDefaultValue(false);

        // ── T01/T07/T08: Rubric, decision, pronunciation, scorecard ──
        builder.Property(t => t.RubricJson).HasDefaultValue(string.Empty);
        builder.Property(t => t.DecisionJson).HasDefaultValue(string.Empty);
        builder.Property(t => t.PronunciationReportJson).HasDefaultValue(string.Empty);
        builder.Property(t => t.ScorecardJson).HasDefaultValue(string.Empty);
    }
}

internal sealed class InterviewSessionConfiguration : IEntityTypeConfiguration<InterviewSession>
{
    public void Configure(EntityTypeBuilder<InterviewSession> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.LearnerId).IsRequired().HasMaxLength(50);
        builder.Property(s => s.InterviewProfileId).IsRequired().HasMaxLength(50);
        builder.Property(s => s.JdText).IsRequired();
        builder.Property(s => s.JdAnalysis).HasDefaultValue(string.Empty);
        builder.Property(s => s.InterviewPlan).HasDefaultValue(string.Empty);
        builder.Property(s => s.Type).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.Mode).HasConversion<string>().HasMaxLength(50)
            .HasDefaultValue(InterviewMode.TrainingInterview);
        builder.Property(s => s.State).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.PlannedQuestionCount).HasDefaultValue(0);

        builder.OwnsOne(s => s.Feedback, feedback =>
        {
            feedback.Property(f => f.OverallScore).HasColumnName("FeedbackOverallScore");
            feedback.Property(f => f.CommunicationScore).HasColumnName("FeedbackCommunicationScore");
            feedback.Property(f => f.TechnicalAccuracyScore).HasColumnName("FeedbackTechnicalAccuracyScore");
            feedback.Property(f => f.ConfidenceScore).HasColumnName("FeedbackConfidenceScore");
            feedback.Property(f => f.DetailedFeedbackEn).HasColumnName("FeedbackDetailedEn").HasMaxLength(4000);
            feedback.Property(f => f.DetailedFeedbackVi).HasColumnName("FeedbackDetailedVi").HasMaxLength(4000);
            feedback.Property(f => f.RetryRecommendation).HasColumnName("FeedbackRetryRecommendation").HasMaxLength(1000);

            feedback.Property(f => f.StrengthAreas).HasColumnName("FeedbackStrengthAreas")
                .HasConversion(
                    v => string.Join("|||", v),
                    v => v.Split("|||", StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly());

            feedback.Property(f => f.ImprovementAreas).HasColumnName("FeedbackImprovementAreas")
                .HasConversion(
                    v => string.Join("|||", v),
                    v => v.Split("|||", StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly());

            feedback.Property(f => f.SuggestedPhrases).HasColumnName("FeedbackSuggestedPhrases")
                .HasConversion(
                    v => string.Join("|||", v),
                    v => v.Split("|||", StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly());
        });

        builder.HasMany(s => s.Turns)
            .WithOne()
            .HasForeignKey(t => t.SessionId)
            .OnDelete(DeleteBehavior.Cascade)
            .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.UpdatedAtUtc).IsRequired();
        builder.HasIndex(s => s.LearnerId);
        builder.HasIndex(s => s.InterviewProfileId);
    }
}
