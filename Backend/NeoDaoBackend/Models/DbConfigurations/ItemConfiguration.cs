using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> entity)
    {
        entity.HasKey(e => e.ItemId).HasName("items_pkey");

        entity.ToTable("items");

        entity.Property(e => e.ItemId)
            .HasColumnName("item_id")
            .HasColumnType("uuid")
            .IsRequired();

        entity.Property(e => e.UnrealItemId)
            .HasColumnName("unreal_item_id")
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.InventoryId)
            .HasColumnName("inventory_id")
            .HasColumnType("uuid")
            .IsRequired();

        entity.Property(e => e.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("int")
            .HasDefaultValue(1)
            .IsRequired();

        entity.Property(e => e.ExpirationDate)
            .HasColumnName("expiration_date")
            .HasColumnType("timestamp with time zone");

        entity.Property(e => e.Properties)
            .HasColumnName("properties")
            .HasColumnType("json")
            .IsRequired();
            
        entity.Property(e => e.MetaforceNftId)
            .HasColumnName("metaforce_nft_id")
            .IsRequired(false);

        entity.Property(e => e.NftDescription)
            .HasColumnName("nft_description")
            .IsRequired(false);

        entity.HasOne(e => e.Inventory)
            .WithMany(i => i.Items)
            .HasForeignKey(e => e.InventoryId)
            .HasConstraintName("items_inventory_id_fkey");

        entity.HasIndex(e => e.InventoryId).HasDatabaseName("idx_inventory");
    }
}
