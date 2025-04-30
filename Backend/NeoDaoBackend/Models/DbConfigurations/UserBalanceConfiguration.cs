using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserBalanceConfiguration : IEntityTypeConfiguration<UserBalance>
{
    public void Configure(EntityTypeBuilder<UserBalance> entity)
    {
        entity.ToTable("user_balances");

        entity.HasKey(e => e.UserId);

        entity.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        entity.Property(e => e.SoftAmount)
            .HasColumnName("soft_amount")
            .IsRequired()
            .HasColumnType("decimal(18, 2)");
        
        entity.Property(e => e.HardAmount)
            .HasColumnName("hard_amount")
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        entity.Property(e => e.BitForceAmount)
            .HasColumnName("bit_force_amount")
            .IsRequired()
            .HasColumnType("decimal(36, 18)"); // Изменено 18 знаков после запятой
        
        entity.HasOne(e => e.User)
            .WithOne(u => u.UserBalance)
            .HasForeignKey<UserBalance>(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
