using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCoach.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeakingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpeakingAttempts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LearnerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContentItemId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AudioUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RawTranscript = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    NormalizedTranscript = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    FeedbackTopMistakes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeedbackImprovedAnswer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeedbackPhrasesToReview = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeedbackRetryPrompt = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeakingAttempts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpeakingAttempts_ContentItemId",
                table: "SpeakingAttempts",
                column: "ContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeakingAttempts_LearnerId",
                table: "SpeakingAttempts",
                column: "LearnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpeakingAttempts");
        }
    }
}
