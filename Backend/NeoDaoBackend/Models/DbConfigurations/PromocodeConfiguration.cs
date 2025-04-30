using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class PromocodeConfiguration : IEntityTypeConfiguration<Promocode>
{
    public void Configure(EntityTypeBuilder<Promocode> entity)
    {
        entity.ToTable("promocodes");

        entity.HasKey(e=>e.Code);

        entity.Property(e => e.CountUse)
            .HasColumnName("count_use")
            .IsRequired();
    }
}
