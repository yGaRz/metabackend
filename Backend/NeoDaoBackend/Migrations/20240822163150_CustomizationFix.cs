using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class CustomizationFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_active_customizations_users_user_id",
                table: "user_active_customizations");

            migrationBuilder.DropForeignKey(
                name: "FK_user_available_customizations_users_user_id",
                table: "user_available_customizations");

            migrationBuilder.AddForeignKey(
                name: "FK_user_active_customizations_users_user_id",
                table: "user_active_customizations",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_available_customizations_users_user_id",
                table: "user_available_customizations",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_active_customizations_users_user_id",
                table: "user_active_customizations");

            migrationBuilder.DropForeignKey(
                name: "FK_user_available_customizations_users_user_id",
                table: "user_available_customizations");

            migrationBuilder.AddForeignKey(
                name: "FK_user_active_customizations_users_user_id",
                table: "user_active_customizations",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_user_available_customizations_users_user_id",
                table: "user_available_customizations",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
