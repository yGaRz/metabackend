using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> entity)
    {
        entity.HasKey(e => new { e.InventoryId, e.SlotType }).HasName("equipments_pkey");
        entity.ToTable("equipments");

        entity.HasIndex(e => e.ItemId).HasName("idx_equipment_item_id");

        entity.Property(e => e.InventoryId)
            .HasColumnName("inventory_id")
            .HasColumnType("uuid")
            .IsRequired();

        entity.Property(e => e.SlotType)
            .HasColumnName("slot_type")
            .IsRequired();

        entity.Property(e => e.ItemId)
            .HasColumnName("item_id")
            .HasColumnType("uuid")
            .IsRequired();

        entity.HasOne(e => e.Inventory)
            .WithMany(i => i.Equipments)
            .HasForeignKey(e => e.InventoryId)
            .HasConstraintName("equipments_inventory_id_fkey");

        entity.HasOne(e => e.Item)
            .WithOne()
            .HasForeignKey<Equipment>(e => e.ItemId)
            .HasConstraintName("equipments_item_id_fkey");
    }
}
