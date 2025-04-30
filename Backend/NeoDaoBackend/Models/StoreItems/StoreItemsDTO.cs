using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.StoreItems;

public class StoreItemsDTO
{
    public Guid InternalId { get; set; }
    public string UnrealId { get; set; }
    public CoinType CoinType { get; set; }
    public decimal? Price { get; set; }
    public StoreType StoreType { get; set; }
    public bool IsMultiPurchasable { get; set; }
    public int? BuyPrice { get; set; }
    public int? SellPrice { get; set; }
    public DateTimeOffset Created { get; set; }
    
    public static StoreItemsDTO FromStoreItem(StoreItem item, double PurchaseRate, double SaleRate)
    {
        StoreItemsDTO itemsDTO = new StoreItemsDTO
        {
            InternalId = item.InternalId,
            UnrealId = item.UnrealId,
            CoinType = item.CoinType,
            IsMultiPurchasable = item.IsMultiPurchasable,
            StoreType = item.StoreType,
            Created = item.Created
        };
            
        if (item.StoreType == StoreType.FreeStore)
        {
            itemsDTO.BuyPrice = (int)Math.Ceiling(item.Price * (decimal)PurchaseRate);
            itemsDTO.SellPrice = (int)Math.Ceiling(item.Price * (decimal)SaleRate);
            itemsDTO.Price = null;
        }
        else
        {
            itemsDTO.BuyPrice = null;
            itemsDTO.SellPrice = null;
            itemsDTO.Price = item.Price;
        }
        return itemsDTO;
    }
}