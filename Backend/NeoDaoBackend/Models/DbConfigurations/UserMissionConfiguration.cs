using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserMissionConfiguration : IEntityTypeConfiguration<UserMission>
{
    public void Configure(EntityTypeBuilder<UserMission> entity)
    {
        entity.HasKey(e => new { e.UserId, e.MissionId })
            .HasName("user_missions_pkey");

        entity.ToTable("user_missions");

        entity.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        entity.Property(e => e.MissionId)
            .HasColumnName("mission_id")
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.MissionType)
            .HasColumnName("mission_type")
            .HasConversion<int>()
            .IsRequired();

        entity.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        entity.Property(e => e.ExpireTime)
            .HasColumnName("expire_time");

        entity.HasOne(e => e.User)
            .WithMany(u => u.UserMissions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("user_missions_user_id_fkey");
    }
}
