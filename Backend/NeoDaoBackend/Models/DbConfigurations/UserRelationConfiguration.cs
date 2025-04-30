using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserRelationConfiguration : IEntityTypeConfiguration<UserRelation>
{
    public void Configure(EntityTypeBuilder<UserRelation> entity)
    {
        entity.HasKey(e => new { e.UserId, e.RelationUserId })
            .HasName("user_relations_pkey");

        entity.ToTable("user_relations");

        entity.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);

        entity.HasOne(e => e.RelationUser)
            .WithMany()
            .HasForeignKey(e => e.RelationUserId);

        entity.Property(e => e.UserId)
            .HasColumnType("uuid")
            .HasColumnName("user_id")
            .IsRequired();

        entity.Property(e => e.RelationUserId)
            .HasColumnType("uuid")
            .HasColumnName("relation_user_id")
            .IsRequired();

        entity.Property(e => e.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created")
            .IsRequired();

        entity.Property(e => e.FriendStatus)
            .HasColumnName("friend_status")
            .HasConversion<int>()
            .IsRequired();

        entity.Property(e => e.BlockStatus)
            .HasColumnName("block_status")
            .HasConversion<int>()
            .IsRequired();
    }
}
