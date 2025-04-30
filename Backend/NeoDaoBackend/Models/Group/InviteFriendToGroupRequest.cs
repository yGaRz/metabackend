using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Group;

public class InviteFriendToGroupRequest : IInputMessageData
{
    [ValidGuid]
    public Guid FriendUserId { get; set; }
    [ValidGuid]
    public Guid GroupId { get; set; }
}
