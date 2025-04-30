using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Group;

public class AcceptUserJoiningGroupRequest : IInputMessageData
{
    [ValidGuid]
    public Guid UserId { get; set; }
    [ValidGuid]
    public Guid GroupId { get; set; }
    public bool IsAccepted { get; set; }
}
