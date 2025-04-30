using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class PlayerLocationConfiguration : IEntityTypeConfiguration<PlayerLocation>
{
    public void Configure(EntityTypeBuilder<PlayerLocation> entity)
    {
        entity.HasKey(e => e.UserId).HasName("player_location_pkey");

        entity.ToTable("player_locations");

        entity.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        entity.Property(e => e.LevelName)
            .HasColumnName("level_name")
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.XCoordinate)
            .HasColumnName("x_coordinate")
            .HasColumnType("double precision")
            .IsRequired();

        entity.Property(e => e.YCoordinate)
            .HasColumnName("y_coordinate")
            .HasColumnType("double precision")
            .IsRequired();

        entity.Property(e => e.ZCoordinate)
            .HasColumnName("z_coordinate")
            .HasColumnType("double precision")
            .IsRequired();

        entity.Property(e => e.Tag)
            .HasColumnName("tag")
            .HasColumnType("text");

        entity.HasOne(d => d.User)
            .WithOne()
            .HasForeignKey<PlayerLocation>(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("player_location_user_id_fkey");
    }
}
