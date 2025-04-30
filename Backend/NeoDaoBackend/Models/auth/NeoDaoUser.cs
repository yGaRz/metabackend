using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.Auth;

public class NeoDaoUser
{
    public Guid? InternalSessionId { get; set; }
    public Guid? ExternalSessionId { get; set; }
    public SessionStatus? Status { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public Guid? UserId { get; set; }
    public Guid? InventoryId { get; set; }
    public string? UserName { get; set; }
    public string? Token { get; set; }
    public UserRole Role { get; set; }

    public NeoDaoUser()
    {
        Role = UserRole.NOT_AUTHENTICATED;
    }

    public NeoDaoUser(Session session)
    {
        InternalSessionId = session.InternalSessionId;
        ExternalSessionId = session.ExternalSessionId;
        ConfirmedAt = session.ConfirmedAt;
        UserId = session.UserId;
        Token = session.Token;
        Role = session.ConfirmedAt != null && session.Token != null ? UserRole.AUTHENTICATED : UserRole.SESSION_CREATED;
        Status = session.Status;
    }
}
