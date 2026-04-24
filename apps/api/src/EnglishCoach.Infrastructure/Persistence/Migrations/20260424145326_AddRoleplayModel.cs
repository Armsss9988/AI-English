using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCoach.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleplayModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleplaySessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LearnerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScenarioId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScenarioContentVersion = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SummaryResult = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SummaryClearPoints = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SummaryTopMistakes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SummaryImprovedAnswer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SummaryPhrasesToReview = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SummaryRetryChallenge = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleplaySessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleplayTurns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AudioUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleplayTurns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleplayTurns_RoleplaySessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "RoleplaySessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleplaySessions_LearnerId",
                table: "RoleplaySessions",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayTurns_SessionId",
                table: "RoleplayTurns",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleplayTurns");

            migrationBuilder.DropTable(
                name: "RoleplaySessions");
        }
    }
}
