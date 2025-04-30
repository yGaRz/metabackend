using NeoDaoBackend.Models.UserRelations;
using Newtonsoft.Json;

namespace NeoDaoBackend.Models.UserRelations.Search;

public class UserIdName
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public FriendStatus? FriendStatus { get; set; }
    public BlockStatus? BlockStatus { get; set; }
    public bool IsOnline { get; set; } = false;
}
