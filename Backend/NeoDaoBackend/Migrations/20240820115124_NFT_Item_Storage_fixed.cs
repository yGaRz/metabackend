using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class NFT_Item_Storage_fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nft_slots_items_inventory_item_id",
                table: "nft_slots");
            
            migrationBuilder.AlterColumn<Guid>(
                name: "inventory_item_id",
                table: "nft_slots",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_nft_slots_items_inventory_item_id",
                table: "nft_slots",
                column: "inventory_item_id",
                principalTable: "items",
                principalColumn: "item_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nft_slots_items_inventory_item_id",
                table: "nft_slots");
            

            migrationBuilder.AlterColumn<Guid>(
                name: "inventory_item_id",
                table: "nft_slots",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_nft_slots_items_inventory_item_id",
                table: "nft_slots",
                column: "inventory_item_id",
                principalTable: "items",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
