using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Group;

public class YouLeavedGroupResponse:IOutputMessageData
{
    public Guid GroupId { get; set; }
    public bool Deleted { get; set; }
}
