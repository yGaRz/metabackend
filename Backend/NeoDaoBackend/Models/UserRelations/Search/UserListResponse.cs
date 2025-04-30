using NeoDaoBackend.Models.WsMessage;
using Newtonsoft.Json;

namespace NeoDaoBackend.Models.UserRelations.Search;

public class UserListResponse : IOutputMessageData
{
    public List<UserIdName> Users { get; set; } = null!;
}
