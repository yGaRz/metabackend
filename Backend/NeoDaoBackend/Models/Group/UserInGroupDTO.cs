using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeoDaoBackend.Models.Group;

public class UserInGroupDTO
{
    [JsonIgnore]
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    [JsonConverter(typeof(StringEnumConverter))]
    public UserGroupStatus UserStatus { get; set; }
    public bool IsLeader { get; set; }
}
