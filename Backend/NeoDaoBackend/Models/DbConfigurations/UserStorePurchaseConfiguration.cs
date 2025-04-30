using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserStorePurchaseConfiguration : IEntityTypeConfiguration<UserStorePurchase>
{
    public void Configure(EntityTypeBuilder<UserStorePurchase> entity)
    {
        entity.ToTable("user_store_purchases");

        entity.HasKey(e => new { e.UserId, e.StoreItemId })
            .HasName("PK_user_store_purchases");

        entity.Property(e => e.UserId)
            .HasColumnName("user_id");

        entity.Property(e => e.StoreItemId)
            .HasColumnName("store_item_id");

        entity.Property(e => e.Created)
            .HasColumnName("created")
            .HasDefaultValueSql("current_timestamp")
            .IsRequired();

        entity.HasOne(e => e.User)
            .WithMany(u => u.UserStorePurchases)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.StoreItem)
            .WithMany(s => s.UserStorePurchases)
            .HasForeignKey(e => e.StoreItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}