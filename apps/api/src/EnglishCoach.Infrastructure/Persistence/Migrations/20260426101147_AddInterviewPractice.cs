using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCoach.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewPractice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewProfiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LearnerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CvText = table.Column<string>(type: "text", nullable: false),
                    CvAnalysis = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterviewSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LearnerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InterviewProfileId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    JdText = table.Column<string>(type: "text", nullable: false),
                    JdAnalysis = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    InterviewPlan = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlannedQuestionCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FeedbackOverallScore = table.Column<int>(type: "integer", nullable: true),
                    FeedbackCommunicationScore = table.Column<int>(type: "integer", nullable: true),
                    FeedbackTechnicalAccuracyScore = table.Column<int>(type: "integer", nullable: true),
                    FeedbackConfidenceScore = table.Column<int>(type: "integer", nullable: true),
                    FeedbackDetailedEn = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    FeedbackDetailedVi = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    FeedbackStrengthAreas = table.Column<string>(type: "text", nullable: true),
                    FeedbackImprovementAreas = table.Column<string>(type: "text", nullable: true),
                    FeedbackSuggestedPhrases = table.Column<string>(type: "text", nullable: true),
                    FeedbackRetryRecommendation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterviewTurns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    AudioUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TurnOrder = table.Column<int>(type: "integer", nullable: false),
                    QuestionCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewTurns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewTurns_InterviewSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "InterviewSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewProfiles_LearnerId",
                table: "InterviewProfiles",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSessions_InterviewProfileId",
                table: "InterviewSessions",
                column: "InterviewProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSessions_LearnerId",
                table: "InterviewSessions",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewTurns_SessionId",
                table: "InterviewTurns",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewProfiles");

            migrationBuilder.DropTable(
                name: "InterviewTurns");

            migrationBuilder.DropTable(
                name: "InterviewSessions");
        }
    }
}
