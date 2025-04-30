using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;
using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Models.DbConfigurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> entity)
    {
        entity.HasKey(key => key.MessageId).HasName("messages_pkey");

        entity.ToTable("chat_messages");

        entity.HasOne(e => e.Sender)
            .WithMany()
            .HasForeignKey(e => e.SenderId);

        entity.HasOne(e => e.Channel)
            .WithMany()
            .HasForeignKey(e => e.ChannelId);

        entity.Property(e => e.MessageId)
            .HasColumnType("bigint")
            .HasColumnName("message_id")
            .IsRequired();

        entity.Property(e => e.ChannelId)
            .HasColumnType("text")
            .HasColumnName("channel_id")
            .IsRequired();

        entity.Property(e => e.SenderId)
            .HasColumnType("uuid")
            .HasColumnName("sender_id");

        entity.Property(e => e.Message)
            .HasColumnType("text")
            .HasColumnName("message")
            .IsRequired();

        entity.Property(e => e.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created")
            .IsRequired();

        // Seed Data
        entity.HasData(
            new ChatMessage()
            {
                ChannelId = GreetingsChatId,
                Created = DateTimeOffset.MinValue,
                Message = "Welcome to Neo Dao",
                MessageId = 1,
                SenderId = null
            }
        );
    }
}
