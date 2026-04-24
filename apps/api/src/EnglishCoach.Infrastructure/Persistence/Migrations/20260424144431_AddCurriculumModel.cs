using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCoach.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurriculumModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Phrases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ViMeaning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CommunicationFunction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Example = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ContentVersion = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phrases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleplayScenarios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WorkplaceContext = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    UserRole = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ClientPersona = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CommunicationGoal = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MustCoverPoints = table.Column<string>(type: "text", nullable: false),
                    TargetPhraseIds = table.Column<string>(type: "text", nullable: false),
                    PassCriteria = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    ContentVersion = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleplayScenarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Phrases_CommunicationFunction",
                table: "Phrases",
                column: "CommunicationFunction");

            migrationBuilder.CreateIndex(
                name: "IX_Phrases_Level",
                table: "Phrases",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Phrases_State",
                table: "Phrases",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayScenarios_State",
                table: "RoleplayScenarios",
                column: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Phrases");

            migrationBuilder.DropTable(
                name: "RoleplayScenarios");
        }
    }
}
