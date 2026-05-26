using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisan.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexOnStatusLastActivityAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_sessions_status_last_activity_at",
                table: "sessions",
                columns: new[] { "status", "last_activity_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sessions_status_last_activity_at",
                table: "sessions");
        }
    }
}
