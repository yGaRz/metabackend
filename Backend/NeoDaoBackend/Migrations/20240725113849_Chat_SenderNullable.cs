using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class Chat_SenderNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chat_messages_users_sender_id",
                table: "chat_messages");

            migrationBuilder.AlterColumn<Guid>(
                name: "sender_id",
                table: "chat_messages",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.InsertData(
                table: "channels",
                columns: new[] { "ChannelId", "userA", "userB" },
                values: new object[,]
                {
                    { "admin", null, null },
                    { "greetings", null, null }
                });

            migrationBuilder.InsertData(
                table: "chat_messages",
                columns: new[] { "message_id", "channel_id", "created", "message", "sender_id" },
                values: new object[] { 1L, "greetings", new DateTimeOffset(new DateTime(2024, 7, 25, 11, 38, 49, 309, DateTimeKind.Unspecified).AddTicks(1641), new TimeSpan(0, 0, 0, 0, 0)), "Welcome to Neo Dao", null });

            migrationBuilder.AddForeignKey(
                name: "FK_chat_messages_users_sender_id",
                table: "chat_messages",
                column: "sender_id",
                principalTable: "users",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chat_messages_users_sender_id",
                table: "chat_messages");

            migrationBuilder.DeleteData(
                table: "channels",
                keyColumn: "ChannelId",
                keyValue: "admin");

            migrationBuilder.DeleteData(
                table: "chat_messages",
                keyColumn: "message_id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "channels",
                keyColumn: "ChannelId",
                keyValue: "greetings");

            migrationBuilder.AlterColumn<Guid>(
                name: "sender_id",
                table: "chat_messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_chat_messages_users_sender_id",
                table: "chat_messages",
                column: "sender_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
