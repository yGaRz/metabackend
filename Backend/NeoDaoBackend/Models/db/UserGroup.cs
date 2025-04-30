using NeoDaoBackend.Models.Group;

namespace NeoDaoBackend.Models.db;

public class UserGroup
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public UserGroupStatus UserStatus { get; set; }
    public DateTimeOffset Created { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Group Group { get; set; } = null!;
}
