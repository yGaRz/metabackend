using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Group;

public class JoinFriendGroupRequest : IInputMessageData
{
    [ValidGuid]
    public Guid UserId { get; set; }
}
