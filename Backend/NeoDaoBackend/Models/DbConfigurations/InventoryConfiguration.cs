using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> entity)
    {
        entity.HasKey(e => e.InventoryId).HasName("inventory_pkey");

        entity.ToTable("inventory");

        entity.HasIndex(e => e.UserId, "idx_unique_user").IsUnique();

        entity.Property(e => e.InventoryId)
            .HasDefaultValueSql("uuid_generate_v4()")
            .HasColumnName("inventory_id")
            .HasColumnType("uuid")
            .IsRequired();

        entity.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        entity.HasOne(e => e.User)
            .WithOne(p => p.Inventory)
            .HasForeignKey<Inventory>(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("inventory_user_id_fkey");

        entity.HasMany(e => e.Items)
            .WithOne(i => i.Inventory)
            .HasForeignKey(i => i.InventoryId)
            .HasConstraintName("items_inventory_id_fkey");

        entity.HasMany(e => e.Equipments)
            .WithOne(equipment => equipment.Inventory)
            .HasForeignKey(equipment => equipment.InventoryId)
            .HasConstraintName("equipment_inventory_id_fkey");
    }
}
