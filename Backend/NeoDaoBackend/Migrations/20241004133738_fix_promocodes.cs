using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class fix_promocodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "auction_id",
                table: "promocodes",
                newName: "count_use");

            migrationBuilder.AlterColumn<int>(
                name: "count_use",
                table: "promocodes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "count_use",
                table: "promocodes",
                newName: "auction_id");

            migrationBuilder.AlterColumn<long>(
                name: "auction_id",
                table: "promocodes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
