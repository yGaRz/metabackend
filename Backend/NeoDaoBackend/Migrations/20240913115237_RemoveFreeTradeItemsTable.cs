using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFreeTradeItemsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Удаление таблицы "free_trade_items"
            migrationBuilder.DropTable(
                name: "free_trade_items");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Откат: создание таблицы "free_trade_items" обратно
            migrationBuilder.CreateTable(
                name: "free_trade_items",
                columns: table => new
                {
                    UnrealId = table.Column<string>(type: "text", nullable: false),
                    NetCost = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_free_trade_items", x => x.UnrealId);
                });
        }
    }
}
