using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class NFTSlot_Refactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "nft_slots");

            migrationBuilder.AlterColumn<decimal>(
                name: "initial_price",
                table: "nft_slots",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "auction_id",
                table: "nft_slots",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_nft_slots_auction_id",
                table: "nft_slots",
                column: "auction_id");

            migrationBuilder.AddForeignKey(
                name: "FK_nft_slots_auctions_auction_id",
                table: "nft_slots",
                column: "auction_id",
                principalTable: "auctions",
                principalColumn: "auction_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nft_slots_auctions_auction_id",
                table: "nft_slots");

            migrationBuilder.DropIndex(
                name: "IX_nft_slots_auction_id",
                table: "nft_slots");

            migrationBuilder.DropColumn(
                name: "auction_id",
                table: "nft_slots");

            migrationBuilder.AlterColumn<int>(
                name: "initial_price",
                table: "nft_slots",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "nft_slots",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
