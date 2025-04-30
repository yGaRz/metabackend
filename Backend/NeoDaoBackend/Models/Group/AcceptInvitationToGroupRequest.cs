using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Group;

public class AcceptInvitationToGroupRequest : IInputMessageData
{
    [ValidGuid]
    public Guid GroupId { get; set; }
    public bool IsAccepted { get; set; }
}
