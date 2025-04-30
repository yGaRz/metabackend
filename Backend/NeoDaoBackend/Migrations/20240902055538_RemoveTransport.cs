using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTransport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_transports_transports_transport_id",
                table: "user_transports");

            migrationBuilder.DropTable(
                name: "transports");

            migrationBuilder.DropIndex(
                name: "IX_user_transports_transport_id",
                table: "user_transports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transports",
                columns: table => new
                {
                    transport_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transports", x => x.transport_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_transports_transport_id",
                table: "user_transports",
                column: "transport_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_transports_transports_transport_id",
                table: "user_transports",
                column: "transport_id",
                principalTable: "transports",
                principalColumn: "transport_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
