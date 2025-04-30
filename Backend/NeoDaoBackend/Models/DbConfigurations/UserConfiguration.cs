using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("users");
        
        entity.HasKey(e => e.UserId)
            .HasName("users_pkey");
        
        entity.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        
        entity.Property(e => e.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created")
            .IsRequired();
        
        entity.Property(e => e.LastLoginAt)
            .HasColumnName("last_login_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
        
        entity.Property(e => e.UserName)
            .HasColumnName("user_name")
            .IsRequired();
        
        entity.Property(e => e.IsOnline)
            .HasColumnName("is_online")
            .IsRequired();
    }
}