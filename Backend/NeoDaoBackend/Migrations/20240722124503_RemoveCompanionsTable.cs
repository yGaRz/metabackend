using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompanionsTable : Migration
    {
        /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Удаляем таблицу companions
        migrationBuilder.DropTable(
            name: "companions");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Восстанавливаем таблицу companions при откате миграции
        migrationBuilder.CreateTable(
            name: "companions",
            columns: table => new
            {
                companion_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                companion_kind = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                companion_type = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("companions_pkey", x => x.companion_id);
            });

        // Восстанавливаем столбец companion_id в таблице items
        migrationBuilder.AddColumn<Guid>(
            name: "companion_id",
            table: "items",
            type: "uuid",
            nullable: true);

        // Восстанавливаем индекс для столбца companion_id
        migrationBuilder.CreateIndex(
            name: "IX_items_companion_id",
            table: "items",
            column: "companion_id");

        // Восстанавливаем внешний ключ, который ссылается на таблицу companions
        migrationBuilder.AddForeignKey(
            name: "FK_items_companions_companion_id",
            table: "items",
            column: "companion_id",
            principalTable: "companions",
            principalColumn: "companion_id");
    }
    }
}
