using NeoDaoBackend.Models.UserRelations;

namespace NeoDaoBackend.Models.db;

public class UserRelation
{
    public Guid UserId { get; set; }
    public Guid RelationUserId { get; set; }
    public DateTimeOffset Created { get; set; }
    public FriendStatus FriendStatus { get; set; }
    public BlockStatus BlockStatus { get; set; }

    public virtual User RelationUser { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}