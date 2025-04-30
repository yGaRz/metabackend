using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class NFTStore_CoinType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "coin_type",
                table: "store_items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "coin_type",
                table: "store_items");
        }
    }
}
