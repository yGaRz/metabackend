using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class FreeTradeItemConfiguration : IEntityTypeConfiguration<FreeTradeItem>
{
    public void Configure(EntityTypeBuilder<FreeTradeItem> entity)
    {
        entity.ToTable("free_trade_items");

        entity.HasKey(e => e.UnrealId);

        entity.Property(e => e.UnrealId)
            .HasColumnName("unreal_id")
            .IsRequired();

        entity.Property(e => e.NetCost)
            .HasColumnName("net_cost")
            .IsRequired();
    }
}
