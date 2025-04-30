using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UserInventory_Next_Version : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "items_clothes_id_fkey",
                table: "items");

            migrationBuilder.DropIndex(
                name: "IX_items_clothes_id",
                table: "items");

            migrationBuilder.DropIndex(
                name: "IX_items_companion_id",
                table: "items");

            migrationBuilder.DropColumn(
                name: "clothes_id",
                table: "items");

            migrationBuilder.DropColumn(
                name: "companion_id",
                table: "items");

            migrationBuilder.DropColumn(
                name: "level",
                table: "items");

            migrationBuilder.DropColumn(
                name: "power",
                table: "items");

            migrationBuilder.RenameColumn(
                name: "item_type",
                table: "items",
                newName: "unreal_item_id");

            migrationBuilder.AlterColumn<int>(
                name: "quantity",
                table: "items",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<Guid>(
                name: "item_id",
                table: "items",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuid_generate_v4()");

            migrationBuilder.AddColumn<string>(
                name: "properties",
                table: "items",
                type: "json",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "properties",
                table: "items");

            migrationBuilder.RenameColumn(
                name: "unreal_item_id",
                table: "items",
                newName: "item_type");

            migrationBuilder.AlterColumn<int>(
                name: "quantity",
                table: "items",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<Guid>(
                name: "item_id",
                table: "items",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuid_generate_v4()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "clothes_id",
                table: "items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "companion_id",
                table: "items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "level",
                table: "items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "power",
                table: "items",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_items_clothes_id",
                table: "items",
                column: "clothes_id");

            migrationBuilder.CreateIndex(
                name: "IX_items_companion_id",
                table: "items",
                column: "companion_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "items_clothes_id_fkey",
                table: "items",
                column: "clothes_id",
                principalTable: "clothing_items",
                principalColumn: "clothes_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
