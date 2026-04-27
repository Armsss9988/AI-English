using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCoach.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewVoicePipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AudioDurationMs",
                table: "InterviewTurns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AudioStorageKey",
                table: "InterviewTurns",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConfirmedTranscript",
                table: "InterviewTurns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DecisionJson",
                table: "InterviewTurns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "LearnerEditedTranscript",
                table: "InterviewTurns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PronunciationReportJson",
                table: "InterviewTurns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RawTranscript",
                table: "InterviewTurns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RubricJson",
                table: "InterviewTurns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScorecardJson",
                table: "InterviewTurns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetCapability",
                table: "InterviewTurns",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TranscriptConfidence",
                table: "InterviewTurns",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "TurnState",
                table: "InterviewTurns",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Created");

            migrationBuilder.AddColumn<string>(
                name: "TurnType",
                table: "InterviewTurns",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationStatus",
                table: "InterviewTurns",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unverified");

            migrationBuilder.AddColumn<string>(
                name: "Mode",
                table: "InterviewSessions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "TrainingInterview");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioDurationMs",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "AudioStorageKey",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "ConfirmedTranscript",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "DecisionJson",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "LearnerEditedTranscript",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "PronunciationReportJson",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "RawTranscript",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "RubricJson",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "ScorecardJson",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "TargetCapability",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "TranscriptConfidence",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "TurnState",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "TurnType",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "InterviewSessions");
        }
    }
}
