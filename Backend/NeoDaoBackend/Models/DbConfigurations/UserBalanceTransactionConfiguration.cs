using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserBalanceTransactionConfiguration : IEntityTypeConfiguration<UserBalanceTransaction>
{
    public void Configure(EntityTypeBuilder<UserBalanceTransaction> entity)
    {
        entity.ToTable("user_balance_transactions");

        entity.HasKey(key => key.TransactionId).HasName("transactions_pkey");

        entity.Property(e => e.TransactionId)
            .HasColumnType("bigint")
            .HasColumnName("transaction_id")
            .IsRequired();

        entity.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        entity.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);
        
        // Добавляем CoinType
        entity.Property(e => e.CoinType)
            .HasColumnName("coin_type")
            .HasColumnType("int")
            .IsRequired();

        entity.Property(e => e.AmountDifference)
            .HasColumnName("amount_difference")
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        entity.Property(e => e.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created")
            .IsRequired();

        entity.Property(e => e.Reason)
            .HasColumnName("reason")
            .HasColumnType("text")
            .IsRequired();
    }
}
