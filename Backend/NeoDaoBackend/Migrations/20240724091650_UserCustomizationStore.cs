using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UserCustomizationStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customization_store",
                columns: table => new
                {
                    internal_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    customization_id = table.Column<string>(type: "text", nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    price = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("customization_store_pkey", x => x.internal_id);
                });

            migrationBuilder.CreateTable(
                name: "user_customization_purchases",
                columns: table => new
                {
                    user_customization_purchase_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    internal_customization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_customizations_purchases_pkey", x => x.user_customization_purchase_id);
                    table.ForeignKey(
                        name: "user_customizations_purchases_internal_customization_id_fkey",
                        column: x => x.internal_customization_id,
                        principalTable: "customization_store",
                        principalColumn: "internal_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_customizations_purchases_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_customizations",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot_type = table.Column<int>(type: "integer", nullable: false),
                    internal_customization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    properties = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_customizations", x => new { x.user_id, x.slot_type });
                    table.ForeignKey(
                        name: "users_customizations_internal_customization_id_fkey",
                        column: x => x.internal_customization_id,
                        principalTable: "customization_store",
                        principalColumn: "internal_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "users_customizations_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_customization_id",
                table: "customization_store",
                column: "customization_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_customization_purchases_internal_customization_id",
                table: "user_customization_purchases",
                column: "internal_customization_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserCustomizationPurchases_UserId_InternalCustomizationId",
                table: "user_customization_purchases",
                columns: new[] { "user_id", "internal_customization_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_customizations_internal_customization_id",
                table: "user_customizations",
                column: "internal_customization_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_customization_purchases");

            migrationBuilder.DropTable(
                name: "user_customizations");

            migrationBuilder.DropTable(
                name: "customization_store");
        }
    }
}
