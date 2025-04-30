using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class UserEmotionStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "emotions",
                columns: table => new
                {
                    internal_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    emotion_id = table.Column<string>(type: "text", nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    price = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("emotions_pkey", x => x.internal_id);
                });

            migrationBuilder.CreateTable(
                name: "user_emotions_purchases",
                columns: table => new
                {
                    emotions_purchases_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    internal_emotion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_emotions_purchases_pkey", x => x.emotions_purchases_id);
                    table.ForeignKey(
                        name: "user_emotions_purchases_emotion_id_fkey",
                        column: x => x.internal_emotion_id,
                        principalTable: "emotions",
                        principalColumn: "internal_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "user_emotions_purchases_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "uq_emotion_id",
                table: "emotions",
                column: "emotion_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_emotions_purchases_internal_emotion_id",
                table: "user_emotions_purchases",
                column: "internal_emotion_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_emotions_purchases_user_id_internal_emotion_id",
                table: "user_emotions_purchases",
                columns: new[] { "user_id", "internal_emotion_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_emotions_purchases");

            migrationBuilder.DropTable(
                name: "emotions");
        }
    }
}
