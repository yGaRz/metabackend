using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClothingStoreAndUserClothesPurchases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "slot_type",
                table: "clothing_items");

            migrationBuilder.RenameTable(
                name: "clothing_items",
                newName: "clothing_store_items");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created",
                table: "user_clothes_purchases",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created",
                table: "user_clothes_purchases");

            migrationBuilder.RenameTable(
                name: "clothing_store_items",
                newName: "clothing_items");

            migrationBuilder.AddColumn<int>(
                name: "slot_type",
                table: "clothing_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
