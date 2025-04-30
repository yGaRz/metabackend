using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace NeoDaoBackend.Models.DbConfigurations;
using NeoDaoBackend.Models.db;

public class StreamConfiguration : IEntityTypeConfiguration<Stream>
{
    public void Configure(EntityTypeBuilder<Stream> entity)
    {
        entity.ToTable("streams");

        entity.HasKey(e => e.StreamId);

        entity.Property(e => e.StreamId)
            .HasColumnName("stream_id");

        entity.Property(e => e.Url)
            .HasColumnName("url")
            .IsRequired();

        entity.Property(e => e.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        entity.Property(e => e.EndTime)
            .HasColumnName("end_time");

        entity.Property(e => e.Created)
            .HasColumnName("created")
            .HasDefaultValueSql("current_timestamp")
            .IsRequired();
    }
}
