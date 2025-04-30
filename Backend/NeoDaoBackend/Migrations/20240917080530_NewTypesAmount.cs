using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class NewTypesAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "bit_force_amount",
                table: "user_balances",
                type: "numeric(36,18)", // 36 знаков всего, и 18 из которых — после запятой
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "hard_amount",
                table: "user_balances",
                type: "numeric(18,2)", // 18 знаков всего, 2 из которых — после запятой
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bit_force_amount",
                table: "user_balances");

            migrationBuilder.DropColumn(
                name: "hard_amount",
                table: "user_balances");
        }
    }
}
