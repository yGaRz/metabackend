using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserAvatarConfiguration : IEntityTypeConfiguration<UserAvatar>
{
    public void Configure(EntityTypeBuilder<UserAvatar> entity)
    {
        entity.HasKey(e => e.UserAvatarId).HasName("user_avatars_pkey");

        entity.ToTable("user_avatars");

        entity.HasIndex(e => e.UserId, "user_avatars_user_id_idx");

        entity.Property(e => e.UserAvatarId)
            .HasDefaultValueSql("uuid_generate_v4()")
            .HasColumnName("user_avatar_id");
        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created");
        entity.Property(e => e.Gender).HasColumnName("avatar_gender");

        entity.HasOne(e => e.User).WithMany(p => p.UserAvatars)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("user_avatars_user_id_fkey");
    }
}