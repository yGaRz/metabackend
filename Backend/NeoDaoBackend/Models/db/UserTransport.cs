namespace NeoDaoBackend.Models.db;

public class UserTransport
{
    public Guid UserId { get; set; }
    public string TransportUnrealId { get; set; } = null!;
    public DateTimeOffset Created { get; set; }

    public virtual User User { get; set; } = null!;    
}