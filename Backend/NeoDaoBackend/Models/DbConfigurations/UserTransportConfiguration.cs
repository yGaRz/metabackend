using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserTransportConfiguration : IEntityTypeConfiguration<db.UserTransport>
{
    public void Configure(EntityTypeBuilder<db.UserTransport> entity)
    {
        entity.HasKey(e => new { e.UserId, e.TransportUnrealId })
            .HasName("user_transport_pkey");

        entity.ToTable("user_transports");

        entity.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);  

        entity.Property(e => e.UserId)
            .HasColumnType("uuid")
            .HasColumnName("user_id")
            .IsRequired();

        entity.Property(e => e.TransportUnrealId)
            .HasColumnType("text")
            .HasColumnName("transport_id")
            .IsRequired();

        entity.Property(e => e.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created")
            .IsRequired();
    }
}