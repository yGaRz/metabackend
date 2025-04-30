using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Group;

public class UserWithNameMessage : IOutputMessageData
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
}
