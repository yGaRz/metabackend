namespace NeoDaoBackend.Models.db;

public class UserStorePurchase
{
    public Guid UserId { get; set; }
    public Guid StoreItemId { get; set; }
    public DateTimeOffset Created { get; set; }
    
    public virtual User User { get; set; }
    public virtual StoreItem StoreItem { get; set; }
}