using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Group;

public class YouJoinedGroupMessage : IOutputMessageData
{
    public Guid GroupId { get; set; }
    public bool IsJoined { get; set; }
}
