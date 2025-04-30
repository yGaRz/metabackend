using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Group;

public class CreateGroupResponse : IOutputMessageData
{
    public Guid GroupId { get; set; }
}
