using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserActiveCustomizationConfiguration : IEntityTypeConfiguration<UserActiveCustomization>
{
    public void Configure(EntityTypeBuilder<UserActiveCustomization> entity)
    {
        entity.ToTable("user_active_customizations");

        entity.HasKey(e => new { e.UserId, e.SlotType });

        entity.Property(e => e.UserId)
            .HasColumnName("user_id");

        entity.Property(e => e.UnrealId)
            .HasColumnName("unreal_id");

        entity.Property(e => e.SlotType)
            .HasColumnName("slot_type");
            
        entity.HasOne(e => e.AvailableCustomization)
            .WithMany()
            .HasForeignKey(e => new { e.UserId, e.UnrealId })
            .HasConstraintName("FK_user_active_customizations_user_available_customizations")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
