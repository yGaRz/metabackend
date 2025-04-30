using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UserBalance_Refactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_financial_transactions");

            migrationBuilder.RenameColumn(
                name: "soft_difference",
                table: "user_balance_transactions",
                newName: "amount_difference");

            migrationBuilder.AddColumn<int>(
                name: "coin_type",
                table: "user_balance_transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.DropTable(
                name: "auction_bids");

            migrationBuilder.AddColumn<Guid>(
                name: "current_bid_user_id",
                table: "auction_lots",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "current_price",
                table: "auction_lots",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_updated",
                table: "auction_lots",
                type: "timestamptz",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateIndex(
                name: "auction_lots_current_bid_user_idx",
                table: "auction_lots",
                column: "current_bid_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_auction_lots_users_current_bid_user_id",
                table: "auction_lots",
                column: "current_bid_user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "coin_type",
                table: "user_balance_transactions");

            migrationBuilder.RenameColumn(
                name: "amount_difference",
                table: "user_balance_transactions",
                newName: "soft_difference");

            migrationBuilder.CreateTable(
                name: "user_financial_transactions",
                columns: table => new
                {
                    transaction_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_difference = table.Column<decimal>(type: "numeric", nullable: false),
                    coin_type = table.Column<int>(type: "int", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("financial_transactions_pkey", x => x.transaction_id);
                    table.ForeignKey(
                        name: "FK_user_financial_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_financial_transactions_user_id",
                table: "user_financial_transactions",
                column: "user_id");
            
            migrationBuilder.DropForeignKey(
                name: "FK_auction_lots_users_current_bid_user_id",
                table: "auction_lots");

            migrationBuilder.DropIndex(
                name: "auction_lots_current_bid_user_idx",
                table: "auction_lots");

            migrationBuilder.DropColumn(
                name: "current_bid_user_id",
                table: "auction_lots");

            migrationBuilder.DropColumn(
                name: "current_price",
                table: "auction_lots");

            migrationBuilder.DropColumn(
                name: "last_updated",
                table: "auction_lots");

            migrationBuilder.CreateTable(
                name: "auction_bids",
                columns: table => new
                {
                    bid_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_bid_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionLotLotId = table.Column<Guid>(type: "uuid", nullable: true),
                    coin_type = table.Column<int>(type: "int", nullable: false),
                    current_price = table.Column<decimal>(type: "numeric", nullable: false),
                    last_updated = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
        }
    }
}
