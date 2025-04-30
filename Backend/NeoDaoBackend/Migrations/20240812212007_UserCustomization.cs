using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UserCustomization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_available_customizations",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unreal_id = table.Column<string>(type: "text", nullable: false),
                    properties = table.Column<string>(type: "json", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_available_customizations", x => new { x.user_id, x.unreal_id });
                    table.ForeignKey(
                        name: "FK_user_available_customizations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_active_customizations",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot_type = table.Column<int>(type: "integer", nullable: false),
                    unreal_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_active_customizations", x => new { x.user_id, x.slot_type });
                    table.ForeignKey(
                        name: "FK_user_active_customizations_user_available_customizations",
                        columns: x => new { x.user_id, x.unreal_id },
                        principalTable: "user_available_customizations",
                        principalColumns: new[] { "user_id", "unreal_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_active_customizations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_active_customizations_user_id_unreal_id",
                table: "user_active_customizations",
                columns: new[] { "user_id", "unreal_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_active_customizations");

            migrationBuilder.DropTable(
                name: "user_available_customizations");
        }
    }
}
