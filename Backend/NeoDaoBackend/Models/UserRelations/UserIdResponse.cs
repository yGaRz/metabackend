using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.UserRelations;

public class UserIdResponse : IOutputMessageData
{
    public Guid UserId { get; set; }
}
