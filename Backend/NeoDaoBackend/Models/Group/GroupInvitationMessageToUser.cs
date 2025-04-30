using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Group;

public class GroupInvitationMessageToUser : IOutputMessageData
{
    public Guid GroupId { get; set; }
    public Guid GroupLeaderUserId { get; set; }
    public string GroupLeaderUserName { get; set; } = null!;
}
