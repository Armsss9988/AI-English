using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCoach.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotebookModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotebookEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LearnerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatternKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OriginalExample = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CorrectedExample = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ExplanationVi = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RecurrenceCount = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EvidenceRefs = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotebookEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotebookEntries_LearnerId_PatternKey",
                table: "NotebookEntries",
                columns: new[] { "LearnerId", "PatternKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotebookEntries_State",
                table: "NotebookEntries",
                column: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotebookEntries");
        }
    }
}
