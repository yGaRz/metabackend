using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;
using static NeoDaoBackend.Models.Constants;

namespace NeoDaoBackend.Models.DbConfigurations;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> entity)
    {
        entity.HasKey(key => key.ChannelId).HasName("channels_pkey");

        entity.ToTable("channels");

        entity.HasOne(e => e.UserA)
            .WithMany()
            .HasForeignKey(e => e.UserAId);

        entity.HasOne(e => e.UserB)
            .WithMany()
            .HasForeignKey(e => e.UserBId);

        entity.Property(e => e.UserAId)
            .HasColumnType("uuid")
            .HasColumnName("userA");

        entity.Property(e => e.UserBId)
            .HasColumnType("uuid")
            .HasColumnName("userB");

        entity.Property(e => e.isReadA)
            .HasColumnType("boolean")
            .HasColumnName("isReadA")
            .HasDefaultValue(false)
            .IsRequired();

        entity.Property(e => e.isReadB)
            .HasColumnType("boolean")
            .HasColumnName("isReadB")
            .HasDefaultValue(false)
            .IsRequired();

        entity.Property(e => e.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created")
            .IsRequired();

        entity.Property(e => e.Updated)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("updated")
            .IsRequired();

        // Seed Data
        entity.HasData(
            new Channel()
            {
                ChannelId = GlobalChatId,
                UserA = null,
                UserB = null
            },
            new Channel()
            {
                ChannelId = AdminChatId,
                UserA = null,
                UserB = null
            },
            new Channel()
            {
                ChannelId = GreetingsChatId,
                UserA = null,
                UserB = null
            }
        );
    }
}