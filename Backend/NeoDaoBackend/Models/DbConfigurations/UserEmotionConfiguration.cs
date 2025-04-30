using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class UserEmotionConfiguration : IEntityTypeConfiguration<UserEmotion>
{
    public void Configure(EntityTypeBuilder<UserEmotion> entity)
    {
        entity.ToTable("user_emotions");
            
        entity.Property(e => e.UserId)
            .HasColumnName("user_id");
            
        entity.Property(e => e.UnrealId)
            .HasColumnName("unreal_id");
            
        entity.HasKey(e => new { e.UserId, e.UnrealId })
            .HasName("PK_user_emotion_unreal");
            
        entity.Property(e => e.Created)
            .HasColumnName("created")
            .HasDefaultValueSql("current_timestamp")
            .IsRequired();
            
        entity.HasOne(e => e.User)
            .WithMany(u => u.UserEmotions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
