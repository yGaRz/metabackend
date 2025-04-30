namespace NeoDaoBackend.Models.db;

public class UserBalance
{
    public Guid UserId { get; set; }
    public decimal SoftAmount { get; set; }
    public decimal HardAmount { get; set; }
    public decimal BitForceAmount { get; set; }

    public virtual User User { get; set; } = null!;
}