using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UserClothingStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "clothes_id",
                table: "items",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "clothing_items",
                columns: table => new
                {
                    clothes_id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    slot_type = table.Column<int>(type: "integer", nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("clothing_items_pkey", x => x.clothes_id);
                });

            migrationBuilder.CreateTable(
                name: "user_clothes_purchases",
                columns: table => new
                {
                    clothes_purchases_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    clothes_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_clothes_purchases_pkey", x => x.clothes_purchases_id);
                    table.ForeignKey(
                        name: "user_clothes_purchases_clothing_item_id_fkey",
                        column: x => x.clothes_id,
                        principalTable: "clothing_items",
                        principalColumn: "clothes_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_clothes_purchases_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_items_clothes_id",
                table: "items",
                column: "clothes_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_clothes_purchases_clothes_id",
                table: "user_clothes_purchases",
                column: "clothes_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_clothes_purchases_user_id",
                table: "user_clothes_purchases",
                column: "user_id");
            
            // Adding unique index on (user_id, clothes_id)
            migrationBuilder.CreateIndex(
                name: "IX_user_clothes_purchases_user_id_clothes_id",
                table: "user_clothes_purchases",
                columns: new[] { "user_id", "clothes_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "items_clothes_id_fkey",
                table: "items",
                column: "clothes_id",
                principalTable: "clothing_items",
                principalColumn: "clothes_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "items_clothes_id_fkey",
                table: "items");

            migrationBuilder.DropTable(
                name: "user_clothes_purchases");

            migrationBuilder.DropTable(
                name: "clothing_items");

            migrationBuilder.DropIndex(
                name: "IX_items_clothes_id",
                table: "items");

            migrationBuilder.DropColumn(
                name: "clothes_id",
                table: "items");
        }
    }
}
