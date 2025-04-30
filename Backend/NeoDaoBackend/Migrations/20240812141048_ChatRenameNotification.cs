using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class ChatRenameNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "notificationB",
                table: "channels",
                newName: "isReadB");

            migrationBuilder.RenameColumn(
                name: "notificationA",
                table: "channels",
                newName: "isReadA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isReadB",
                table: "channels",
                newName: "notificationB");

            migrationBuilder.RenameColumn(
                name: "isReadA",
                table: "channels",
                newName: "notificationA");
        }
    }
}
