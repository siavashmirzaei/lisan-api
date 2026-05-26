using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisan.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTranscriptsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transcripts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    turn = table.Column<int>(type: "integer", nullable: false),
                    speaker = table.Column<string>(type: "text", nullable: false),
                    text_fa = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transcripts", x => x.id);
                    table.ForeignKey(
                        name: "FK_transcripts_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transcripts_session_id",
                table: "transcripts",
                column: "session_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transcripts");
        }
    }
}
