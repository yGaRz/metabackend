using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Group;

public class UserGroupData : IOutputMessageData
{
    public List<UserGroupDTO> userGroups { get; set; } = null!;
}
