namespace NeoDaoBackend.Models.db;

public class Group
{
    public Guid GroupId { get; set; }
    public Guid LeaderUserId { get; set; }
    public DateTimeOffset Created { get; set; }

    public virtual User Leader { get; set; } = null!;
}
