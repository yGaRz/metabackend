using NeoDaoBackend.Models.WsMessage;
using Newtonsoft.Json;

namespace NeoDaoBackend.Models.UserRelations;

public class UserIdRequest : IInputMessageData
{
    public Guid UserId { get; set; }
}
