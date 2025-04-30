using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.DbConfigurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> entity)
    {
        entity.HasKey(e => e.InternalSessionId).HasName("user_sessions_pkey");

        entity.ToTable("user_sessions");

        entity.HasIndex(e => e.UserId, "user_sessions_user_id_idx");

        entity.Property(e => e.InternalSessionId).HasColumnName("internal_session_id");
        entity.Property(e => e.ExternalSessionId).HasColumnName("external_session_id");
        entity.Property(e => e.ConfirmedAt).HasColumnName("confirmed_at");
        entity.Property(e => e.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created");
        entity.Property(e => e.RemovedAt).HasColumnName("removed_at");
        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.Token).HasColumnName("token");
        entity.Property(e => e.Status)
            .HasDefaultValue(SessionStatus.CREATED)
            .HasColumnName("status");
        entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
        
        entity.Property(e => e.OpenMatchTicket)
            .HasColumnName("open_match_ticket")
            .HasColumnType("text");
        
        entity.HasOne(e => e.User).WithMany(p => p.Sessions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("user_sessions_user_id_fkey");
    }
}