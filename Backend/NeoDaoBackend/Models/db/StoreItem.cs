namespace NeoDaoBackend.Models.db;

public class StoreItem
{
    public Guid InternalId { get; set; }
    public string UnrealId { get; set; }
    public StoreType StoreType { get; set; }
    public CoinType CoinType { get; set; }
    public decimal Price { get; set; }
    public bool IsMultiPurchasable { get; set; }
    public DateTimeOffset Created { get; set; } 
    
    public virtual ICollection<UserStorePurchase> UserStorePurchases { get; set; } = new List<UserStorePurchase>();
}