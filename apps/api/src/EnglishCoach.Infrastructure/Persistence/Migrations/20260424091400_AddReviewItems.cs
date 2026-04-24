using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCoach.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "review_items",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    item_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    review_track = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    display_text = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    display_subtitle = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    mastery_state = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    repetition_count = table.Column<int>(type: "integer", nullable: false),
                    due_at_utc = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<long>(type: "bigint", nullable: false),
                    updated_at_utc = table.Column<long>(type: "bigint", nullable: false),
                    last_completed_at_utc = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "review_attempts",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    review_item_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    quality = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    previous_state = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    next_state = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    previous_repetition_count = table.Column<int>(type: "integer", nullable: false),
                    next_repetition_count = table.Column<int>(type: "integer", nullable: false),
                    completed_at_utc = table.Column<long>(type: "bigint", nullable: false),
                    next_due_at_utc = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_attempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_attempts_review_items_review_item_id",
                        column: x => x.review_item_id,
                        principalTable: "review_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_review_attempts_review_item_id",
                table: "review_attempts",
                column: "review_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_items_user_id_item_id_review_track",
                table: "review_items",
                columns: new[] { "user_id", "item_id", "review_track" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "review_attempts");

            migrationBuilder.DropTable(
                name: "review_items");
        }
    }
}
