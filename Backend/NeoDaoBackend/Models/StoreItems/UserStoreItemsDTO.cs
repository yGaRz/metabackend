using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Models.StoreItems;

public class UserStoreItemsDTO
{
    public Guid InternalId { get; set; }
    public string UnrealId { get; set; }
    public CoinType CoinType { get; set; }
    public decimal? Price { get; set; }
    public bool IsMultiPurchasable { get; set; }
    public bool IsBought { get; set; }
    public decimal? BuyPrice { get; set; }
    public decimal? SellPrice { get; set; }

    public static UserStoreItemsDTO FromUserStoreItem(StoreItem item, double PurchaseRate, double SaleRate)
    {
        UserStoreItemsDTO itemsDTO = new UserStoreItemsDTO();
        itemsDTO.InternalId = item.InternalId;
        itemsDTO.UnrealId = item.UnrealId;
        itemsDTO.IsMultiPurchasable = item.IsMultiPurchasable;
        itemsDTO.CoinType = item.CoinType;
            
        if(item.StoreType == StoreType.FreeStore)
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