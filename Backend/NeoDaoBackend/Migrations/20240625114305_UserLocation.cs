using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UserLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "player_locations",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    level_name = table.Column<string>(type: "text", nullable: false),
                    x_coordinate = table.Column<double>(type: "double precision", nullable: false),
                    y_coordinate = table.Column<double>(type: "double precision", nullable: false),
                    z_coordinate = table.Column<double>(type: "double precision", nullable: false),
                    tag = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("player_location_pkey", x => x.user_id);
                    table.ForeignKey(
                        name: "player_location_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_locations");
        }
    }
}
