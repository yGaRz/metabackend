using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class CompanionsAndEquipments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "companion_id",
                table: "items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "companions",
                columns: table => new
                {
                    companion_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    companion_kind = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    companion_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("companions_pkey", x => x.companion_id);
                });

            migrationBuilder.CreateTable(
                name: "equipments",
                columns: table => new
                {
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot_type = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("equipments_pkey", x => new { x.inventory_id, x.slot_type });
                    table.ForeignKey(
                        name: "equipments_inventory_id_fkey",
                        column: x => x.inventory_id,
                        principalTable: "inventory",
                        principalColumn: "inventory_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "equipments_item_id_fkey",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_items_companion_id",
                table: "items",
                column: "companion_id");

            migrationBuilder.CreateIndex(
                name: "idx_equipment_item_id",
                table: "equipments",
                column: "item_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_items_companions_companion_id",
                table: "items",
                column: "companion_id",
                principalTable: "companions",
                principalColumn: "companion_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_items_companions_companion_id",
                table: "items");

            migrationBuilder.DropTable(
                name: "companions");

            migrationBuilder.DropTable(
                name: "equipments");

            migrationBuilder.DropIndex(
                name: "IX_items_companion_id",
                table: "items");

            migrationBuilder.DropColumn(
                name: "companion_id",
                table: "items");
        }
    }
}
