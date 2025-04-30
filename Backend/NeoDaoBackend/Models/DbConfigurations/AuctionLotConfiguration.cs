using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class AuctionLotConfiguration : IEntityTypeConfiguration<AuctionLot>
{
    public void Configure(EntityTypeBuilder<AuctionLot> entity)
    {
        entity.ToTable("auction_lots"); // Соответствие названию таблицы

        entity.HasKey(e => e.LotId)
            .HasName("PK_auction_lots");

        entity.Property(e => e.LotId)
            .HasColumnName("lot_id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        entity.Property(e => e.InventoryItemId)
            .HasColumnName("inventory_item_id")
            .IsRequired(false); // InventoryItemId может быть null

        entity.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        entity.Property(e => e.Created)
            .HasColumnName("created")
            .HasDefaultValueSql("current_timestamp");

        entity.Property(e => e.CoinType)
            .HasColumnName("coin_type")
            .HasColumnType("int")
            .IsRequired();

        entity.Property(e => e.InitialPrice)
            .HasColumnName("initial_price")
            .IsRequired();

        entity.Property(e => e.AuctionId)
            .HasColumnName("auction_id")
            .IsRequired();

        // Поля из AuctionBid, сделаны nullable
        entity.Property(e => e.CurrentBidUserId)
            .HasColumnName("current_bid_user_id")
            .HasColumnType("uuid")
            .IsRequired(false); // CurrentBidUserId может быть null

        entity.Property(e => e.CurrentPrice)
            .HasColumnName("current_price")
            .IsRequired(false); // CurrentPrice может быть null

        entity.Property(e => e.LastUpdated)
            .HasColumnName("last_updated")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired(false); // LastUpdated может быть null

        // Навигационные свойства и связи
        entity.HasOne(e => e.Auction)
            .WithMany(a => a.AuctionLots) // Изменено на AuctionLots для связи
            .HasForeignKey(e => e.AuctionId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.InventoryItem)
            .WithOne()
            .HasForeignKey<AuctionLot>(e => e.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.User)
            .WithMany(u => u.AuctionLots) // Изменено на AuctionLots
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.CurrentBidUser) // Связь с пользователем, который сделал ставку
            .WithMany()
            .HasForeignKey(e => e.CurrentBidUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Индексы
        entity.HasIndex(e => e.InventoryItemId)
            .HasDatabaseName("auction_lots_inventory_item_idx")
            .IsUnique();

        entity.HasIndex(e => e.UserId)
            .HasDatabaseName("auction_lots_user_idx");

        entity.HasIndex(e => e.CurrentBidUserId)
            .HasDatabaseName("auction_lots_current_bid_user_idx"); // Новый индекс для CurrentBidUserId
    }
}