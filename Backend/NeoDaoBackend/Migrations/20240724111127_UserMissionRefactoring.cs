using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UserMissionRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_completed",
                table: "user_missions");

            migrationBuilder.DropColumn(
                name: "objective_id",
                table: "user_missions");

            migrationBuilder.DropColumn(
                name: "progress",
                table: "user_missions");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expire_time",
                table: "user_missions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "mission_type",
                table: "user_missions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "user_missions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "mission_objectives",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mission_id = table.Column<string>(type: "text", nullable: false),
                    objective_id = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    metadata = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mission_objectives_pkey", x => new { x.user_id, x.mission_id, x.objective_id });
                    table.ForeignKey(
                        name: "FK_mission_objectives_user_missions_user_id_mission_id",
                        columns: x => new { x.user_id, x.mission_id },
                        principalTable: "user_missions",
                        principalColumns: new[] { "user_id", "mission_id" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mission_objectives");

            migrationBuilder.DropColumn(
                name: "expire_time",
                table: "user_missions");

            migrationBuilder.DropColumn(
                name: "mission_type",
                table: "user_missions");

            migrationBuilder.DropColumn(
                name: "status",
                table: "user_missions");

            migrationBuilder.AddColumn<bool>(
                name: "is_completed",
                table: "user_missions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "objective_id",
                table: "user_missions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "progress",
                table: "user_missions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
