using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCoach.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLearnerProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "learner_profiles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    native_language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    current_english_level = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    target_use_case = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    target_timeline_weeks = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learner_profiles", x => x.user_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "learner_profiles");
        }
    }
}
