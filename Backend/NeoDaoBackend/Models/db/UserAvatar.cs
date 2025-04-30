using System.Text.Json.Serialization;

namespace NeoDaoBackend.Models.db;

public class UserAvatar {
    public Guid UserAvatarId { get; set; }
    public Guid UserId { get; set; }
    public AvatarGender Gender { get; set; }
    public DateTimeOffset Created { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
