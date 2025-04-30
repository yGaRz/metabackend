namespace NeoDaoBackend.Models.db;

public class Channel
{
    public string ChannelId { get; set; } = null!;
    public Guid? UserAId {  get; set; }
    public Guid? UserBId { get; set; }
    public bool isReadA {  get; set; }
    public bool isReadB { get; set; }
    public DateTimeOffset Updated { get; set; }
    public DateTimeOffset Created { get; set; }

    public virtual User? UserA { get; set; }
    public virtual User? UserB { get; set; }
}
