using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisan.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParentAndChildProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "active_persona_id",
                table: "child_profiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "age",
                table: "child_profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "child_profiles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "daily_time_limit_minutes",
                table: "child_profiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "child_profiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "parent_id",
                table: "child_profiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "sessions_completed",
                table: "child_profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "speaking_ladder_stage",
                table: "child_profiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "parents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    clerk_user_id = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    consent_accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    consent_version = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_child_profiles_parent_id",
                table: "child_profiles",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_parents_clerk_user_id",
                table: "parents",
                column: "clerk_user_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_child_profiles_parents_parent_id",
                table: "child_profiles",
                column: "parent_id",
                principalTable: "parents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_child_profiles_parents_parent_id",
                table: "child_profiles");

            migrationBuilder.DropTable(
                name: "parents");

            migrationBuilder.DropIndex(
                name: "ix_child_profiles_parent_id",
                table: "child_profiles");

            migrationBuilder.DropColumn(
                name: "active_persona_id",
                table: "child_profiles");

            migrationBuilder.DropColumn(
                name: "age",
                table: "child_profiles");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "child_profiles");

            migrationBuilder.DropColumn(
                name: "daily_time_limit_minutes",
                table: "child_profiles");

            migrationBuilder.DropColumn(
                name: "name",
                table: "child_profiles");

            migrationBuilder.DropColumn(
                name: "parent_id",
                table: "child_profiles");

            migrationBuilder.DropColumn(
                name: "sessions_completed",
                table: "child_profiles");

            migrationBuilder.DropColumn(
                name: "speaking_ladder_stage",
                table: "child_profiles");
        }
    }
}
