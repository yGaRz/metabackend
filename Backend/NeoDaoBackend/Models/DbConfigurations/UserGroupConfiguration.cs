using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> entity)
    {
        entity.ToTable("user_groups");

        entity.HasKey(e => new { e.GroupId, e.UserId });

        entity.Property(e => e.GroupId)
            .HasColumnName("group_id")
            .IsRequired();
        entity.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        entity.Property(e => e.UserStatus)
            .HasColumnName("user_status")
            .HasConversion<int>()
            .IsRequired();
        entity.Property(e => e.Created)
            .HasColumnName("created")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        entity.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.Group)
            .WithMany()
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(e => new { e.UserId })
            .HasFilter($"user_status = 0")
            .IsUnique();
    }
}
