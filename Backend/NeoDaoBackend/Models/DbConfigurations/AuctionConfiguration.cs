using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> entity)
    {
        entity.ToTable("auctions");

        entity.HasKey(e => e.AuctionId);

        entity.Property(e => e.AuctionId)
            .HasColumnName("auction_id")
            .IsRequired();

        entity.Property(e => e.LotsStartTime)
            .HasColumnName("lots_start_time")
            .HasDefaultValueSql("current_timestamp");

        entity.Property(e => e.AuctionStartTime)
            .HasColumnName("auction_start_time")
            .HasDefaultValueSql("current_timestamp");

        entity.Property(e => e.AuctionEndTime)
            .HasColumnName("auction_end_time")
            .HasDefaultValueSql("current_timestamp");

        // Настройка связи с лотами (опционально, так как уже сделано в AuctionLot)
        entity.HasMany(a => a.AuctionLots)
            .WithOne(s => s.Auction)
            .HasForeignKey(s => s.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}