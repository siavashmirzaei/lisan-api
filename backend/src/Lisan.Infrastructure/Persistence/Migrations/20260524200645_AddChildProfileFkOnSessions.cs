using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisan.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChildProfileFkOnSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "child_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_child_profiles", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_child_profiles_child_profile_id",
                table: "sessions",
                column: "child_profile_id",
                principalTable: "child_profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sessions_child_profiles_child_profile_id",
                table: "sessions");

            migrationBuilder.DropTable(
                name: "child_profiles");
        }
    }
}
