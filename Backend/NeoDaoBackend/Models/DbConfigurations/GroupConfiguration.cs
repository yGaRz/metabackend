using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NeoDaoBackend.Models.DbConfigurations;

public class GroupConfiguration : IEntityTypeConfiguration<db.Group>
{
    public void Configure(EntityTypeBuilder<db.Group> entity)
    {
        entity.ToTable("groups");

        entity.HasKey(e => e.GroupId);

        entity.Property(e => e.GroupId)
            .HasColumnName("group_id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();
        entity.Property(e => e.LeaderUserId)
            .HasColumnName("leader_user_id")
            .IsRequired();
        entity.Property(e => e.Created)
            .HasColumnName("created")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        entity.HasOne(e => e.Leader)
            .WithMany()
            .HasForeignKey(e => e.LeaderUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
