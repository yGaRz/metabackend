using System.Text.Json.Serialization;

namespace NeoDaoBackend.Models.db;

public class Session {
    public Guid InternalSessionId { get; set; }

    public Guid ExternalSessionId { get; set; }

    public SessionStatus Status { get; set; }

    public Guid? UserId { get; set; }

    public string? Token { get; set; }

    public DateTimeOffset ExpiredAt { get; set; }

    public DateTimeOffset? ConfirmedAt { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? RemovedAt { get; set; }
    public string? OpenMatchTicket { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; } = null!;

    public static Session GetDevelopmentSession(Guid userId, string token)
    {
        return new Session()
        {
            ExternalSessionId = Guid.NewGuid(),
            InternalSessionId = Guid.NewGuid(),
            Status = SessionStatus.CONFIRMED,
            ExpiredAt = DateTimeOffset.UtcNow.AddYears(100),
            Created = DateTimeOffset.UtcNow,
            ConfirmedAt = DateTimeOffset.UtcNow,
            Token = token,
            UserId = userId
        };
    }
}
