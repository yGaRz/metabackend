using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class MissionObjectivesConfiguration : IEntityTypeConfiguration<MissionObjectives>
{
    public void Configure(EntityTypeBuilder<MissionObjectives> entity)
    {
        entity.HasKey(e => new { e.UserId, e.MissionId, e.ObjectiveId })
            .HasName("mission_objectives_pkey");

        entity.ToTable("mission_objectives");

        entity.HasOne(e => e.UserMission)
            .WithMany(e => e.Objectives)
            .HasForeignKey(e => new { e.UserId, e.MissionId });

        entity.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        entity.Property(e => e.MissionId)
            .HasColumnName("mission_id")
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.ObjectiveId)
            .HasColumnName("objective_id")
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();
    }
}
