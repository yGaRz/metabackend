using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NeoDaoBackend.Migrations
{
    /// <inheritdoc />
    public partial class Chat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    ChannelId = table.Column<string>(type: "text", nullable: false),
                    userA = table.Column<Guid>(type: "uuid", nullable: true),
                    userB = table.Column<Guid>(type: "uuid", nullable: true),
                    notificationA = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    notificationB = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("channels_pkey", x => x.ChannelId);
                    table.ForeignKey(
                        name: "FK_channels_users_userA",
                        column: x => x.userA,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_channels_users_userB",
                        column: x => x.userB,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    message_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    channel_id = table.Column<string>(type: "text", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("messages_pkey", x => x.message_id);
                    table.ForeignKey(
                        name: "FK_chat_messages_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "channels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chat_messages_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "channels",
                columns: new[] { "ChannelId", "userA", "userB" },
                values: new object[] { "global", null, null });

            migrationBuilder.CreateIndex(
                name: "IX_channels_userA",
                table: "channels",
                column: "userA");

            migrationBuilder.CreateIndex(
                name: "IX_channels_userB",
                table: "channels",
                column: "userB");

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_channel_id",
                table: "chat_messages",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_sender_id",
                table: "chat_messages",
                column: "sender_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "channels");
        }
    }
}
