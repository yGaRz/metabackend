﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AllStores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "store_items",
                columns: table => new
                {
                    internal_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    unreal_id = table.Column<string>(type: "text", nullable: false),
                    store_type = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_store_items", x => x.internal_id);
                });

            migrationBuilder.CreateTable(
                name: "user_store_purchases",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_store_purchases", x => new { x.user_id, x.store_item_id });
                    table.ForeignKey(
                        name: "FK_user_store_purchases_store_items_store_item_id",
                        column: x => x.store_item_id,
                        principalTable: "store_items",
                        principalColumn: "internal_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_store_purchases_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_store_purchases_store_item_id",
                table: "user_store_purchases",
                column: "store_item_id");
            
            // Adding unique index on unreal_id column
            migrationBuilder.CreateIndex(
                name: "IX_store_items_unreal_id",
                table: "store_items",
                column: "unreal_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_store_purchases");

            migrationBuilder.DropTable(
                name: "store_items");
        }
    }
}