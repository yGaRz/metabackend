using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class StoreItemConfiguration : IEntityTypeConfiguration<StoreItem>
{
    public void Configure(EntityTypeBuilder<StoreItem> entity)
    {
        entity.ToTable("store_items");

        entity.HasKey(e => e.InternalId)
            .HasName("PK_store_items");

        entity.Property(e => e.InternalId)
            .HasColumnName("internal_id")
            .HasDefaultValueSql("uuid_generate_v4()");

        entity.Property(e => e.UnrealId)
            .HasColumnName("unreal_id")
            .IsRequired();
            
        entity.HasIndex(e => e.UnrealId)
            .IsUnique()
            .HasDatabaseName("IX_store_items_unreal_id");

        entity.Property(e => e.StoreType)
            .HasColumnName("store_type")
            .HasConversion<int>()
            .IsRequired();
        
        entity.Property(e => e.CoinType)
            .HasColumnName("coin_type")
            .HasColumnType("int")
            .IsRequired();

        entity.Property(e => e.Price)
            .HasColumnName("price")
            .IsRequired();
        
        // Новое свойство IsMultiPurchasable
        entity.Property(e => e.IsMultiPurchasable)
            .HasColumnName("is_multi_purchasable")
            .HasDefaultValue(false) // Значение по умолчанию false
            .IsRequired();

        entity.Property(e => e.Created)
            .HasColumnName("created")
            .HasDefaultValueSql("current_timestamp")
            .IsRequired();
    }
}
