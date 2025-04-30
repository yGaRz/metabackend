using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserAvailableCustomizationConfiguration : IEntityTypeConfiguration<UserAvailableCustomization>
{
    public void Configure(EntityTypeBuilder<UserAvailableCustomization> entity)
    {
        entity.ToTable("user_available_customizations");

        entity.HasKey(e => new { e.UserId, e.UnrealId });

        entity.Property(e => e.UserId)
            .HasColumnName("user_id");

        entity.Property(e => e.UnrealId)
            .HasColumnName("unreal_id");

        entity.Property(e => e.Properties)
            .HasColumnName("properties")
            .HasColumnType("json")
            .IsRequired();

        entity.Property(e => e.Created)
            .HasColumnName("created")
            .HasDefaultValueSql("current_timestamp")
            .IsRequired();
    }
}
