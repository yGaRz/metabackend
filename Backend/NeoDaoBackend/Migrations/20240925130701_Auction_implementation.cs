using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class Auction_implementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nft_slots");

            migrationBuilder.AlterColumn<decimal>(
                name: "soft_amount",
                table: "user_balances",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "soft_difference",
                table: "user_balance_transactions",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "auction_lots",
                columns: table => new
                {
                    lot_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    auction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp"),
                    coin_type = table.Column<int>(type: "int", nullable: false),
                    initial_price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auction_lots", x => x.lot_id);
                    table.ForeignKey(
                        name: "FK_auction_lots_auctions_auction_id",
                        column: x => x.auction_id,
                        principalTable: "auctions",
                        principalColumn: "auction_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_auction_lots_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_auction_lots_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "auction_bids",
                columns: table => new
                {
                    bid_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coin_type = table.Column<int>(type: "int", nullable: false),
                    current_price = table.Column<decimal>(type: "numeric", nullable: false),
                    current_bid_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_updated = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    AuctionLotLotId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("auction_bids_pkey", x => x.bid_id);
                    table.ForeignKey(
                        name: "FK_auction_bids_auction_lots_AuctionLotLotId",
                        column: x => x.AuctionLotLotId,
                        principalTable: "auction_lots",
                        principalColumn: "lot_id");
                    table.ForeignKey(
                        name: "auction_bids_current_bid_user_id_fkey",
                        column: x => x.current_bid_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "auction_bids_lot_id_fkey",
                        column: x => x.lot_id,
                        principalTable: "auction_lots",
                        principalColumn: "lot_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_current_bid_user",
                table: "auction_bids",
                column: "current_bid_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_lot",
                table: "auction_bids",
                column: "lot_id");

            migrationBuilder.CreateIndex(
                name: "IX_auction_bids_AuctionLotLotId",
                table: "auction_bids",
                column: "AuctionLotLotId");

            migrationBuilder.CreateIndex(
                name: "auction_lots_inventory_item_idx",
                table: "auction_lots",
                column: "inventory_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "auction_lots_user_idx",
                table: "auction_lots",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_auction_lots_auction_id",
                table: "auction_lots",
                column: "auction_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auction_bids");

            migrationBuilder.DropTable(
                name: "auction_lots");

            migrationBuilder.AlterColumn<int>(
                name: "soft_amount",
                table: "user_balances",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "soft_difference",
                table: "user_balance_transactions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.CreateTable(
                name: "nft_slots",
                columns: table => new
                {
                    slot_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    auction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coin_type = table.Column<int>(type: "int", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp"),
                    initial_price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nft_slots", x => x.slot_id);
                    table.ForeignKey(
                        name: "FK_nft_slots_auctions_auction_id",
                        column: x => x.auction_id,
                        principalTable: "auctions",
                        principalColumn: "auction_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_nft_slots_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_nft_slots_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nft_slots_auction_id",
                table: "nft_slots",
                column: "auction_id");

            migrationBuilder.CreateIndex(
                name: "nft_slots_inventory_item_idx",
                table: "nft_slots",
                column: "inventory_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "nft_slots_user_idx",
                table: "nft_slots",
                column: "user_id");
        }
    }
}
